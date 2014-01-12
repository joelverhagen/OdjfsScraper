using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Database;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Model.Fetchers;
using OdjfsScraper.Synchronizer.Support;

namespace OdjfsScraper.Synchronizer.Synchronizers
{
    public class CountySynchronizer : ICountySynchronizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IChildCareStubListFetcher _listFetcher;

        public CountySynchronizer(IChildCareStubListFetcher listFetcher)
        {
            _listFetcher = listFetcher;
        }

        public async Task UpdateNextCounty(Entities ctx)
        {
            Logger.Trace("Fetching the next county to scrape.");
            await UpdateCounty(
                ctx,
                counties => counties
                    .OrderBy(c => c.LastScrapedOn.HasValue)
                    .ThenBy(c => c.LastScrapedOn)
                    .FirstOrDefaultAsync());
        }

        public async Task UpdateCounty(Entities ctx, string name)
        {
            Logger.Trace("Fetching the county with Name '{0}' to scrape.", name);
            await UpdateCounty(
                ctx,
                counties => counties
                    .FirstOrDefaultAsync(c => c.Name.ToUpper() == name.ToUpper()));
        }

        private async Task UpdateCounty(Entities ctx, Func<IDbSet<County>, Task<County>> countySelector)
        {
            Logger.Trace("Getting the next County to scrape.");
            County county = await countySelector(ctx.Counties);
            if (county == null)
            {
                Logger.Trace("No county matching the provided selector was found.");
                return;
            }

            Logger.Trace("The next county to scrape is '{0}'.", county.Name);
            await UpdateCounty(ctx, county);
        }

        private async Task UpdateCounty(Entities ctx, County county)
        {
            // record this scrape
            county.LastScrapedOn = DateTime.Now;
            await ctx.SaveChangesAsync();

            // get the stubs from the web
            Logger.Trace("Scraping stubs for county '{0}'.", county.Name);
            ChildCareStub[] webStubs = (await _listFetcher.Fetch(county)).ToArray();
            Logger.Trace("{0} stubs were scraped.", webStubs.Length);

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
                Logger.ErrorException(string.Format(
                    "County: '{0}', HasDuplicates: '{1}', TotalCount: {2}, UniqueCount: {3}",
                    county.Name,
                    string.Join(", ", duplicateIds),
                    webStubs.Length,
                    webIds.Count), exception);
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
            Logger.Trace("{0} stubs were found in the database.", dbStubIds.Count);

            ChildCare[] dbChildCares = await ctx
                .ChildCares
                .Where(c => c.CountyId == county.Id)
                .ToArrayAsync();
            ISet<string> dbIds = new HashSet<string>(dbChildCares.Select(c => c.ExternalUrlId));
            IDictionary<string, ChildCare> idToDbChildCare = dbChildCares.ToDictionary(c => c.ExternalUrlId);
            Logger.Trace("{0} child cares were found in the database.", dbIds.Count);

            if (dbStubIds.Overlaps(dbIds))
            {
                dbStubIds.IntersectWith(dbIds);

                var exception = new SynchronizerException("There are child cares that exist in both the ChildCare and ChildCareStub tables.");
                Logger.ErrorException(string.Format("County: '{0}', Overlapping: '{1}'", county.Name, string.Join(", ", dbStubIds)), exception);
                throw exception;
            }

            dbIds.UnionWith(dbStubIds);

            // find the newly deleted child cares
            ISet<string> deleted = new HashSet<string>(dbIds);
            deleted.ExceptWith(webIds);
            Logger.Trace("{0} child cares or stubs will be deleted.", deleted.Count);

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
            Logger.Trace("{0} stubs will be added.", added.Count);

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
                webStub.Id = dbStub.Id;
                ctx.ChildCareStubs.AddOrUpdate(webStub);
            }
            Logger.Trace("{0} stubs will be updated.", updated.Count);

            Logger.Trace("Saving changes.");
            await ctx.SaveChangesAsync();
        }
    }
}