using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OdjfsScraper.Database;
using OdjfsScraper.Fetch;
using OdjfsScraper.Models;

namespace OdjfsScraper.Synchronize
{
    public class ChildCareSynchronizer : IChildCareSynchronizer
    {
        private readonly IChildCareFetcher _childCareFetcher;
        private readonly ILogger<ChildCareSynchronizer> _logger;

        public ChildCareSynchronizer(ILogger<ChildCareSynchronizer> logger, IChildCareFetcher childCareFetcher)
        {
            _logger = logger;
            _childCareFetcher = childCareFetcher;
        }

        public async Task UpdateNextChildCare(OdjfsContext ctx)
        {
            _logger.LogInformation("Fetching the next stub or child care to scrape.");
            await UpdateChildCareOrStub(
                ctx,
                async childCareStubs =>
                {
                    var notScraped = await childCareStubs
                        .Where(c => !c.LastScrapedOn.HasValue)
                        .Include(x => x.County)
                        .FirstOrDefaultAsync();

                    if (notScraped != null)
                    {
                        return notScraped;
                    }

                    return await childCareStubs
                       .Where(c => c.LastScrapedOn.HasValue)
                       .OrderBy(c => c.LastScrapedOn)
                       .Include(x => x.County)
                       .FirstOrDefaultAsync();
                },
                childCares => childCares
                    .OrderBy(c => c.LastScrapedOn)
                    .Include(x => x.County)
                    .FirstOrDefaultAsync());
        }

        public async Task UpdateChildCare(OdjfsContext ctx, string externalUrlId)
        {
            _logger.LogInformation("Fetching the stub or child care with ExternalUrlId '{externalUrlId}' to scrape.", externalUrlId);
            await UpdateChildCareOrStub(
                ctx,
                childCareStubs => childCareStubs
                    .Include(x => x.County)
                    .FirstOrDefaultAsync(c => c.ExternalUrlId == externalUrlId),
                childCares => childCares
                    .Include(x => x.County)
                    .FirstOrDefaultAsync(c => c.ExternalUrlId == externalUrlId));
        }

        private async Task SetAttachedCountyAsync(OdjfsContext ctx, ChildCare childCare)
        {
            if (childCare.County != null && childCare.County.Id == 0)
            {
                childCare.County = await ctx.Counties.SingleAsync(c => c.Name == childCare.County.Name);
                childCare.CountyId = childCare.County.Id;
            }
        }

        private async Task UpdateChildCareStub(OdjfsContext ctx, ChildCareStub stub)
        {
            // record this scrape
            stub.LastScrapedOn = DateTime.Now;
            await ctx.SaveChangesAsync();

            _logger.LogInformation("Stub with ID '{externalUrlId}' will be scraped.", stub.ExternalUrlId);
            ChildCare newChildCare = await _childCareFetcher.Fetch(stub);
            ctx.ChildCareStubs.Remove(stub);
            if (newChildCare != null)
            {
                await SetAttachedCountyAsync(ctx, newChildCare);
                ctx.ChildCares.Add(newChildCare);
            }
            else
            {
                _logger.LogInformation("There was an permanent error getting the full detail page for the child care.");
                ChildCare existingChildCare = await ctx
                    .ChildCares
                    .Where(c => c.ExternalUrlId == stub.ExternalUrlId)
                    .FirstOrDefaultAsync();
                if (existingChildCare != null)
                {
                    _logger.LogInformation("The associated child care will be deleted.");
                    ctx.ChildCares.Remove(existingChildCare);
                }
            }

            _logger.LogInformation("Saving changes.");
            await ctx.SaveChangesAsync();
        }

        private async Task UpdateChildCare(OdjfsContext ctx, ChildCare oldChildCare)
        {
            // record this scrape
            oldChildCare.LastScrapedOn = DateTime.Now;
            await ctx.SaveChangesAsync();

            _logger.LogInformation("Child care with ID '{externalUrlId}' will be scraped.", oldChildCare.ExternalUrlId);
            ChildCare newChildCare = await _childCareFetcher.Fetch(oldChildCare);
            if (newChildCare != null)
            {
                await SetAttachedCountyAsync(ctx, newChildCare);
            }
            else
            {
                _logger.LogInformation("There was an permanent error getting the full detail page for the child care.");
                ctx.ChildCares.Remove(oldChildCare);
                ChildCareStub stub = await ctx
                    .ChildCareStubs
                    .Where(c => c.ExternalUrlId == oldChildCare.ExternalUrlId)
                    .FirstOrDefaultAsync();
                if (stub != null)
                {
                    _logger.LogInformation("The associated stub was deleted.");
                    ctx.ChildCareStubs.Remove(stub);
                }
            }

            _logger.LogInformation("Saving changes.");
            await ctx.SaveChangesAsync();
        }

        private async Task UpdateChildCareOrStub(OdjfsContext ctx, Func<IQueryable<ChildCareStub>, Task<ChildCareStub>> childCareStubSelector, Func<IQueryable<ChildCare>, Task<ChildCare>> childCareSelector)
        {
            _logger.LogInformation("Getting the child care to scrape.");
            _logger.LogInformation("Checking for a stub matching the selector.");
            ChildCareStub stub = await childCareStubSelector(ctx.ChildCareStubs);

            _logger.LogInformation("Checking for a child care matching the selector.");
            ChildCare childCare = await childCareSelector(ctx.ChildCares);

            if (stub != null && (!stub.LastScrapedOn.HasValue || childCare == null || stub.LastScrapedOn.Value <= childCare.LastScrapedOn))
            {
                _logger.LogInformation("Updating stub with ExternalUrlId '{externalUrlId}'.", stub.ExternalUrlId);
                await UpdateChildCareStub(ctx, stub);
                return;
            }

            if (childCare == null)
            {
                _logger.LogInformation("There are no child care or child care stub records matching the selector to scrape.");
                return;
            }

            await UpdateChildCare(ctx, childCare);
        }
    }
}