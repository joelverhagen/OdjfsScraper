using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Knapcode.PolyGeocoder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Database;
using OdjfsScraper.Models;

namespace OdjfsScraper.Geocode
{
    public class Geocoder : IGeocoder
    {
        private readonly ISimpleGeocoder _simpleGeocoder;
        private readonly ILogger<Geocoder> _logger;

        public Geocoder(ILogger<Geocoder> logger, ISimpleGeocoder simpleGeocoder)
        {
            _logger = logger;
            _simpleGeocoder = simpleGeocoder;
        }

        public async Task<bool> NeedsGeocoding(OdjfsContext ctx)
        {
            int count = await ctx
                .ChildCares
                .Where(c => c.Address != null && (!c.Latitude.HasValue || !c.Longitude.HasValue) && !c.LastGeocodedOn.HasValue)
                .CountAsync();
            return count > 0;
        }

        public async Task GeocodeChildCare(OdjfsContext ctx, string externalUrlId, string mapQuestKey)
        {
            // get the child care in question
            _logger.LogInformation("Fetching child care with ExternalUrlId '{externalUrlId}' to geocode.", externalUrlId);
            externalUrlId = (externalUrlId ?? string.Empty).Trim();
            ChildCare childCare = await ctx
                .ChildCares
                .Where(c => c.ExternalUrlId == externalUrlId)
                .FirstOrDefaultAsync();
            if (childCare == null)
            {
                _logger.LogInformation("No child care with ExternalUrlId '{externalUrlId}' was found.", externalUrlId);
                return;
            }

            await GeocodeChildCare(ctx, childCare, mapQuestKey);
        }

        public async Task GeocodeNextChildCare(OdjfsContext ctx, string mapQuestKey)
        {
            ChildCare childCare = await ctx
                .ChildCares
                .Where(c => c.Address != null && (!c.Latitude.HasValue || !c.Longitude.HasValue) && !c.LastGeocodedOn.HasValue)
                .FirstOrDefaultAsync();
            if (childCare == null)
            {
                _logger.LogInformation("There are no child cares to geocode.");
                return;
            }

            await GeocodeChildCare(ctx, childCare, mapQuestKey);
        }

        private async Task GeocodeChildCare(OdjfsContext ctx, ChildCare childCare, string mapQuestKey)
        {
            if (string.IsNullOrWhiteSpace(mapQuestKey))
            {
                throw new ArgumentException("The MapQuest key is required.", "mapQuestKey");
            }

            _logger.LogInformation("Geocoding child care '{externalUrlId}'.", childCare.ExternalUrlId);

            if (childCare.Address == null)
            {
                _logger.LogInformation("The provided child care does not have an address.");
                return;
            }

            childCare.LastGeocodedOn = DateTime.Now;
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
            _logger.LogInformation("Child care '{externalUrlId}' is at {latitude}, {longitude}.", childCare.ExternalUrlId, geocoderLocation.Latitude, geocoderLocation.Longitude);
            childCare.Latitude = geocoderLocation.Latitude;
            childCare.Longitude = geocoderLocation.Longitude;

            // save the changes
            ctx.ChildCares.Add(childCare);
            _logger.LogInformation("Saving changes.");
            await ctx.SaveChangesAsync();
        }

        private async Task<Location> GetGeocodedLocation(string query)
        {
            _logger.LogInformation("Geocoding address '{query}'.", query);
            Response geocoderResponse = await _simpleGeocoder.GeocodeAsync(query);

            if (geocoderResponse.Locations.Length != 1)
            {
                _logger.LogInformation("Address '{query}' could not be reliably geocoded. GeocodedLocationCount: {locationCount}, GeocodedLocationNames: '{locationNames}'.",
                    query,
                    geocoderResponse.Locations.Length,
                    string.Join(", ", geocoderResponse.Locations.Select(l => l.Name)));
                return null;
            }

            return geocoderResponse.Locations[0];
        }
    }
}