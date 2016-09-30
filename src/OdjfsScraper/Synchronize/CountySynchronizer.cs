using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Database;
using OdjfsScraper.Fetch;
using OdjfsScraper.Models;

namespace OdjfsScraper.Synchronize
{
    public class CountySynchronizer : ICountySynchronizer
    {
        private readonly IChildCareStubListFetcher _listFetcher;
        private readonly ILogger<CountySynchronizer> _logger;

        public CountySynchronizer(ILogger<CountySynchronizer> logger, IChildCareStubListFetcher listFetcher)
        {
            _logger = logger;
            _listFetcher = listFetcher;
        }

        public async Task UpdateNextCounty(OdjfsContext ctx)
        {
            _logger.LogInformation("Fetching the next county to scrape.");
            await UpdateCounty(
                ctx,
                async counties =>
                {
                    var notScraped = await counties
                        .Where(c => !c.LastScrapedOn.HasValue)
                        .FirstOrDefaultAsync();

                    if (notScraped != null)
                    {
                        return notScraped;
                    }

                    return await counties
                       .Where(c => c.LastScrapedOn.HasValue)
                       .OrderBy(c => c.LastScrapedOn)
                       .FirstOrDefaultAsync();
                });
        }

        public async Task UpdateCounty(OdjfsContext ctx, string name)
        {
            _logger.LogInformation("Fetching the county with Name '{name}' to scrape.", name);
            await UpdateCounty(
                ctx,
                counties => counties
                    .FirstOrDefaultAsync(c => c.Name.ToUpper() == name.ToUpper()));
        }

        private async Task UpdateCounty(OdjfsContext ctx, Func<IQueryable<County>, Task<County>> countySelector)
        {
            _logger.LogInformation("Getting the next County to scrape.");
            County county = await countySelector(ctx.Counties);
            if (county == null)
            {
                _logger.LogInformation("No county matching the provided selector was found.");
                return;
            }

            _logger.LogInformation("The next county to scrape is '{name}'.", county.Name);
            await UpdateCounty(ctx, county);
        }

        private async Task UpdateCounty(OdjfsContext ctx, County county)
        {
            // record this scrape
            county.LastScrapedOn = DateTime.Now;
            await ctx.SaveChangesAsync();

            // get the stubs from the web
            _logger.LogInformation("Scraping stubs for county '{name}'.", county.Name);
            ChildCareStub[] webStubs = (await _listFetcher.Fetch(county)).ToArray();
            _logger.LogInformation("{count} stubs were scraped.", webStubs.Length);

            // get the IDs
            ISet<string> webIds = new HashSet<string>(webStubs.Select(c => c.ExternalUrlId));

            if (webStubs.Length != webIds.Count)
            {
                IEnumerable<string> duplicateIds = webStubs
                    .Select(c => c.ExternalUrlId)
                    .GroupBy(i => i)
                    .Select(g => g.ToArray())
                    .Where(g => g.Length > 0)
                    .Select(g => g[0]);

                var exception = new SynchronizerException("One more more duplicate child cares were found in the list.");
                _logger.LogError(
                    exception.Message + " County: '{county}', HasDuplicates: '{duplicateIds}', TotalCount: {totalCount}, UniqueCount: {uniqueCount}",
                    county.Name,
                    string.Join(", ", duplicateIds),
                    webStubs.Length,
                    webIds.Count);
                throw exception;
            }

            // get all of the stub that belong to this county or do not have a county
            // TODO: this code assumes a child care or stub never changed county
            ChildCareStub[] dbStubs = await ctx
                .ChildCareStubs
                .Where(c => c.CountyId == null || c.CountyId == county.Id)
                .ToArrayAsync();
            ISet<string> dbStubIds = new HashSet<string>(dbStubs.Select(s => s.ExternalUrlId));
            IDictionary<string, ChildCareStub> idToDbStub = dbStubs.ToDictionary(s => s.ExternalUrlId);
            _logger.LogInformation("{count} stubs were found in the database.", dbStubIds.Count);

            ChildCare[] dbChildCares = await ctx
                .ChildCares
                .Where(c => c.CountyId == county.Id)
                .ToArrayAsync();
            ISet<string> dbIds = new HashSet<string>(dbChildCares.Select(c => c.ExternalUrlId));
            IDictionary<string, ChildCare> idToDbChildCare = dbChildCares.ToDictionary(c => c.ExternalUrlId);
            _logger.LogInformation("{count} child cares were found in the database.", dbIds.Count);

            if (dbStubIds.Overlaps(dbIds))
            {
                dbStubIds.IntersectWith(dbIds);

                var exception = new SynchronizerException("There are child cares that exist in both the ChildCare and ChildCareStub tables.");
                _logger.LogError(
                    exception.Message + " County: '{count}', Overlapping: '{overlappingIds}'",
                    county.Name,
                    string.Join(", ", dbStubIds));
                throw exception;
            }

            dbIds.UnionWith(dbStubIds);

            // find the newly deleted child cares
            ISet<string> deleted = new HashSet<string>(dbIds);
            deleted.ExceptWith(webIds);
            _logger.LogInformation("{count} child cares or stubs will be deleted.", deleted.Count);

            // delete
            if (deleted.Count > 0)
            {
                foreach (string id in deleted)
                {
                    ChildCareStub stub;
                    if (idToDbStub.TryGetValue(id, out stub))
                    {
                        ctx.ChildCareStubs.Remove(stub);
                    }
                    ChildCare childCare;
                    if (idToDbChildCare.TryGetValue(id, out childCare))
                    {
                        ctx.ChildCares.Remove(childCare);
                    }
                }
            }

            // find the newly added child cares
            ISet<string> added = new HashSet<string>(webIds);
            added.ExceptWith(dbIds);
            _logger.LogInformation("{count} stubs will be added.", added.Count);

            // add
            foreach (ChildCareStub stub in webStubs.Where(c => added.Contains(c.ExternalUrlId)))
            {
                ctx.ChildCareStubs.Add(stub);
            }

            // find stubs that we already have records of
            ISet<string> updated = new HashSet<string>(dbStubIds);
            updated.IntersectWith(webIds);

            // update
            foreach (ChildCareStub webStub in webStubs.Where(c => updated.Contains(c.ExternalUrlId)))
            {
                ChildCareStub dbStub = idToDbStub[webStub.ExternalUrlId];
                dbStub.Address = webStub.Address;
                dbStub.City = webStub.City;
                dbStub.Name = webStub.Name;
            }
            _logger.LogInformation("{count} stubs will be updated.", updated.Count);

            _logger.LogInformation("Saving changes.");
            await ctx.SaveChangesAsync();
        }
    }
}