using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Database;
using OdjfsScraper.Model;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Scraper.Scrapers;
using OdjfsScraper.Scraper.Support;
using PolyGeocoder.Geocoders;
using PolyGeocoder.Support;

namespace OdjfsScraper.DataChecker
{
    public class Odjfs
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IChildCareScraper _childCareScraper;
        private readonly IChildCareStubListScraper _listScraper;

        public Odjfs(IChildCareStubListScraper listScraper, IChildCareScraper childCareScraper)
        {
            _listScraper = listScraper;
            _childCareScraper = childCareScraper;
        }

        private async Task UpdateChildCareStub(Entities ctx, ChildCareStub stub)
        {
            // record this scrape
            stub.LastScrapedOn = DateTime.Now;
            await ctx.SaveChangesAsync();

            Logger.Trace("Stub with ID '{0}' will be scraped.", stub.ExternalUrlId);
            ChildCare newChildCare = await _childCareScraper.Scrape(stub);
            ctx.ChildCareStubs.Remove(stub);
            if (newChildCare != null)
            {
                await SetAttachedCountyAsync(ctx, newChildCare);
                ctx.ChildCares.AddOrUpdate(newChildCare);
            }
            else
            {
                Logger.Trace("There was an permanent error getting the full detail page for the child care.");
                ChildCare existingChildCare = await ctx
                    .ChildCares
                    .Where(c => c.ExternalUrlId == stub.ExternalUrlId)
                    .FirstOrDefaultAsync();
                if (existingChildCare != null)
                {
                    Logger.Trace("The associated child care will be deleted.");
                    ctx.ChildCares.Remove(existingChildCare);
                }
            }

            Logger.Trace("Saving changes.");
            await ctx.SaveChangesAsync();
        }

        private async Task UpdateChildCare(Entities ctx, ChildCare oldChildCare)
        {
            // record this scrape
            oldChildCare.LastScrapedOn = DateTime.Now;
            await ctx.SaveChangesAsync();

            Logger.Trace("Child care with ID '{0}' will be scraped.", oldChildCare.ExternalUrlId);
            ChildCare newChildCare = await _childCareScraper.Scrape(oldChildCare);
            if (newChildCare != null)
            {
                await SetAttachedCountyAsync(ctx, newChildCare);
                ctx.ChildCares.AddOrUpdate(newChildCare);
            }
            else
            {
                Logger.Trace("There was an permanent error getting the full detail page for the child care.");
                ctx.ChildCares.Remove(oldChildCare);
                ChildCareStub stub = await ctx
                    .ChildCareStubs
                    .Where(c => c.ExternalUrlId == oldChildCare.ExternalUrlId)
                    .FirstOrDefaultAsync();
                if (stub != null)
                {
                    Logger.Trace("The associated stub was deleted.");
                    ctx.ChildCareStubs.Remove(stub);
                }
            }

            Logger.Trace("Saving changes.");
            await ctx.SaveChangesAsync();
        }

        private async Task UpdateChildCareOrStub(Entities ctx, Func<IDbSet<ChildCareStub>, Task<ChildCareStub>> childCareStubSelector, Func<IDbSet<ChildCare>, Task<ChildCare>> childCareSelector)
        {
            Logger.Trace("Getting the child care to scrape.");
            Logger.Trace("Checking for a stub matching the selector.");
            ChildCareStub stub = await childCareStubSelector(ctx.ChildCareStubs);

            Logger.Trace("Checking for a child care matching the selector.");
            ChildCare childCare = await childCareSelector(ctx.ChildCares);

            if (stub != null && (!stub.LastScrapedOn.HasValue || childCare == null || stub.LastScrapedOn.Value <= childCare.LastScrapedOn))
            {
                Logger.Trace("Updating stub with ExternalUrlId '{0}'.", stub.ExternalUrlId);
                await UpdateChildCareStub(ctx, stub);
                return;
            }

            if (childCare == null)
            {
                Logger.Trace("There are no child care or child care stub records matching the selector to scrape.");
                return;
            }

            await UpdateChildCare(ctx, childCare);
        }

        public async Task UpdateNextChildCare(Entities ctx)
        {
            Logger.Trace("Fetching the next stub or child care to scrape.");
            await UpdateChildCareOrStub(
                ctx,
                childCareStubs => childCareStubs
                    .OrderBy(c => c.LastScrapedOn.HasValue)
                    .ThenBy(c => c.LastScrapedOn)
                    .FirstOrDefaultAsync(),
                childCares => childCares
                    .OrderBy(c => c.LastScrapedOn)
                    .FirstOrDefaultAsync());
        }

        public async Task UpdateChildCare(Entities ctx, string externalUrlId)
        {
            Logger.Trace("Fetching the stub or child care with ExternalUrlId '{0}' to scrape.", externalUrlId);
            await UpdateChildCareOrStub(
                ctx,
                childCareStubs => childCareStubs
                    .FirstOrDefaultAsync(c => c.ExternalUrlId == externalUrlId),
                childCares => childCares
                    .FirstOrDefaultAsync(c => c.ExternalUrlId == externalUrlId));
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
            ChildCareStub[] webStubs = (await _listScraper.Scrape(county)).ToArray();
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

                var exception = new ScraperException("One more more duplicate child cares were found in the list.");
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

                var exception = new ScraperException("There are child cares that exist in both the ChildCare and ChildCareStub tables.");
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

        private async Task SetAttachedCountyAsync(Entities ctx, ChildCare childCare)
        {
            if (childCare.County != null && childCare.County.Id == 0)
            {
                childCare.County = await ctx.Counties.SingleAsync(c => c.Name == childCare.County.Name);
                childCare.CountyId = childCare.County.Id;
            }
        }

        public async Task<bool> NeedsGeocoding(Entities ctx)
        {
            int count = await ctx
                .ChildCares
                .Where(c => c.Address != null && (!c.Latitude.HasValue || !c.Longitude.HasValue) && !c.LastGeocodedOn.HasValue)
                .CountAsync();
            return count > 0;
        }

        public async Task GeocodeChildCare(Entities ctx, string externalUrlId, string mapQuestKey)
        {
            // get the child care in question
            Logger.Trace("Fetching child care with ExternalUrlId '{0}' to geocode.", externalUrlId);
            externalUrlId = (externalUrlId ?? string.Empty).Trim();
            ChildCare childCare = await ctx
                .ChildCares
                .Where(c => c.ExternalUrlId == externalUrlId)
                .FirstOrDefaultAsync();
            if (childCare == null)
            {
                Logger.Trace("No child care with ExternalUrlId '{0}' was found.", externalUrlId);
                return;
            }

            await GeocodeChildCare(ctx, childCare, mapQuestKey);
        }


        public async Task GeocodeNextChildCare(Entities ctx, string mapQuestKey)
        {
            ChildCare childCare = await ctx
                .ChildCares
                .Where(c => c.Address != null && (!c.Latitude.HasValue || !c.Longitude.HasValue) && !c.LastGeocodedOn.HasValue)
                .FirstOrDefaultAsync();
            if (childCare == null)
            {
                Logger.Trace("There are no child cares to geocode.");
                return;
            }

            await GeocodeChildCare(ctx, childCare, mapQuestKey);
        }

        private async Task GeocodeChildCare(Entities ctx, ChildCare childCare, string mapQuestKey)
        {
            if (string.IsNullOrWhiteSpace(mapQuestKey))
            {
                throw new ArgumentException("The MapQuest key is required.", "mapQuestKey");
            }

            Logger.Trace("Geocoding child care '{0}'.", childCare.ExternalUrlId);

            if (childCare.Address == null)
            {
                Logger.Trace("The provided child care does not have an address.");
                return;
            }

            childCare.LastGeocodedOn = DateTime.Now;
            ctx.ChildCares.AddOrUpdate(childCare);
            ctx.SaveChanges();

            // create the geocoder
            IClient geocoderClient = new Client(ScraperClient.GetUserAgent());
            ISimpleGeocoder geocoder = new MapQuestGeocoder(geocoderClient, MapQuestGeocoder.LicensedEndpoint, mapQuestKey);

            // geocode based off the full address
            Location geocoderLocation = await GetGeocodedLocation(geocoder, string.Join(", ", new[]
            {
                childCare.Address,
                childCare.City,
                childCare.State,
                childCare.ZipCode.ToString(CultureInfo.InvariantCulture)
            }));
            if (geocoderLocation == null)
            {
                geocoderLocation = await GetGeocodedLocation(geocoder, string.Join(", ", new[]
                {
                    childCare.Address,
                    childCare.ZipCode.ToString(CultureInfo.InvariantCulture)
                }));

                if (geocoderLocation == null)
                {
                    return;
                }
            }

            // copy over the latitude and longitude from the response
            Logger.Trace("Child care '{0}' is at {1}, {2}.", childCare.ExternalUrlId, geocoderLocation.Latitude, geocoderLocation.Longitude);
            childCare.Latitude = geocoderLocation.Latitude;
            childCare.Longitude = geocoderLocation.Longitude;

            // save the changes
            ctx.ChildCares.AddOrUpdate(childCare);
            Logger.Trace("Saving changes.");
            await ctx.SaveChangesAsync();
        }

        private async Task<Location> GetGeocodedLocation(ISimpleGeocoder geocoder, string query)
        {
            Logger.Trace("Geocoding address '{0}'.", query);
            Response geocoderResponse = await geocoder.GeocodeAsync(query);

            if (geocoderResponse.Locations.Length != 1)
            {
                Logger.Trace("Address '{0}' could not be reliably geocoded. GeocodedLocationCount: {1}, GeocodedLocationNames: '{2}'.",
                    query,
                    geocoderResponse.Locations.Length,
                    string.Join(", ", geocoderResponse.Locations.Select(l => l.Name)));
                return null;
            }

            return geocoderResponse.Locations[0];
        }
    }
}