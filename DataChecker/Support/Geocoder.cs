using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Database;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Scraper.Support;
using PolyGeocoder.Geocoders;
using PolyGeocoder.Support;

namespace OdjfsScraper.DataChecker.Support
{
    public class Geocoder : IGeocoder
    {
        private readonly ISimpleGeocoder _simpleGeocoder;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Geocoder(ISimpleGeocoder simpleGeocoder)
        {
            _simpleGeocoder = simpleGeocoder;
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

            // geocode based off the full address
            Location geocoderLocation = await GetGeocodedLocation(string.Join(", ", new[]
            {
                childCare.Address,
                childCare.City,
                childCare.State,
                childCare.ZipCode.ToString(CultureInfo.InvariantCulture)
            }));
            if (geocoderLocation == null)
            {
                geocoderLocation = await GetGeocodedLocation(string.Join(", ", new[]
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

        private async Task<Location> GetGeocodedLocation(string query)
        {
            Logger.Trace("Geocoding address '{0}'.", query);
            Response geocoderResponse = await _simpleGeocoder.GeocodeAsync(query);

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