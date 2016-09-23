using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using OdjfsScraper.Database;
using OdjfsScraper.Model.ChildCares;
using OdjfsScraper.Model.ChildCareStubs;
using OdjfsScraper.Model.Fetchers;

namespace OdjfsScraper.Synchronizer.Synchronizers
{
    public class ChildCareSynchronizer : IChildCareSynchronizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IChildCareFetcher _childCareFetcher;

        public ChildCareSynchronizer(IChildCareFetcher childCareFetcher)
        {
            _childCareFetcher = childCareFetcher;
        }

        public async Task UpdateNextChildCare(Entities ctx)
        {
            Logger.Trace("Fetching the next stub or child care to scrape.");
            await UpdateChildCareOrStub(
                ctx,
                async childCareStubs =>
                {
                    var notScraped = await childCareStubs
                        .Where(c => !c.LastScrapedOn.HasValue)
                        .FirstOrDefaultAsync();

                    if (notScraped != null)
                    {
                        return notScraped;
                    }

                    return await childCareStubs
                       .Where(c => c.LastScrapedOn.HasValue)
                       .OrderBy(c => c.LastScrapedOn)
                       .FirstOrDefaultAsync();
                },
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

        private async Task SetAttachedCountyAsync(Entities ctx, ChildCare childCare)
        {
            if (childCare.County != null && childCare.County.Id == 0)
            {
                childCare.County = await ctx.Counties.SingleAsync(c => c.Name == childCare.County.Name);
                childCare.CountyId = childCare.County.Id;
            }
        }

        private async Task UpdateChildCareStub(Entities ctx, ChildCareStub stub)
        {
            // record this scrape
            stub.LastScrapedOn = DateTime.Now;
            await ctx.SaveChangesAsync();

            Logger.Trace("Stub with ID '{0}' will be scraped.", stub.ExternalUrlId);
            ChildCare newChildCare = await _childCareFetcher.Fetch(stub);
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
            ChildCare newChildCare = await _childCareFetcher.Fetch(oldChildCare);
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
    }
}