using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Scrapy.Exceptions;
using Scrapy.Jobs.Extentions;
using Scrapy.Jobs.Services.Interfaces;
using Scrapy.Models;
using Scrapy.Ports;
using Scrapy.Utility.Constants;

namespace Scrapy.Jobs.Services
{
    public class ScrapperService : IScrapperService
    {
        private readonly ITvMazeStore _tvMazeStore;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ScrapperService> _logger;

        public ScrapperService(ITvMazeStore tvMazeStore, IDistributedCache cache, ILogger<ScrapperService> logger)
        {
            _tvMazeStore = tvMazeStore ?? throw new ArgumentNullException(nameof(tvMazeStore));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ScrapAsync(CancellationToken cancellationToken)
        {
            var shows = new List<Show>();
            var pageIndex = 0;
            var isDataAvilable = true;

            try
            {
                while (isDataAvilable)
                {
                    var response = await _tvMazeStore.GetShowsAsync(pageIndex, cancellationToken);

                    if (response is null || !response.Any())
                    {
                        isDataAvilable = false;
                        continue;
                    }

                    shows.AddRange(response.Select(x => x));

                    var showIds = shows
                        .Select(s => s.Id)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    await _cache.SetStringAsync(Constants.CacheKeyForShowIndex, JsonSerializer.Serialize(showIds.Select(s => s).ToArray()), cancellationToken);

                    // Process sub tasks in batches to process 10 requests for getting casts at once
                    var batches = CreateShowBatches(shows, 10);

                    foreach (var batch in batches)
                    {
                        var processShowTasks = batch.Select(x => FillShowsWithCasts(x, cancellationToken));
                        await Task.WhenAll(processShowTasks);
                    }

                    pageIndex++;
                }
            }
            catch (GetShowsExceptions)
            {
                throw;
            }
            catch (GetCastsExceptions)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.ScrappingFailed(ex);

                throw;
            }
        }

        private async Task FillShowsWithCasts(Show show, CancellationToken cancellationToken)
        {
            var getShowResponse = await _tvMazeStore.GetCastAsync(show.Id, cancellationToken);

            if (getShowResponse is null)
            {
                return;
            }

            var showsWithCast = GenerateShowModelWithCasts(show, getShowResponse);

            await _cache.SetStringAsync(
                Constants.GetCacheKeyForShow(showsWithCast.Id),
                JsonSerializer.Serialize(showsWithCast),
                cancellationToken);
        }

        private static Show GenerateShowModelWithCasts(Show show, IEnumerable<Cast> getShowResponse) =>
            new()
            {
                Id = show.Id,
                Name = show.Name,
                Cast = getShowResponse
                        .Select(cast =>
                            new Cast
                            {
                                Person = new Person
                                {
                                    Id = cast.Person.Id,
                                    Name = cast.Person.Name,
                                    Birthday = cast.Person.Birthday,
                                }
                            })
                        .ToList()
            };

        private static IEnumerable<IEnumerable<Show>> CreateShowBatches(ICollection<Show> shows, int batchSize)
        {
            var total = 0;

            while (total < shows.Count)
            {
                yield return shows.Skip(total).Take(batchSize);

                total += batchSize;
            }
        }
    }
}
