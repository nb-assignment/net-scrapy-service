using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Scrapy.Extentions;
using Scrapy.Models;
using Scrapy.UseCases.Interfaces;
using Scrapy.Utility.Constants;

namespace Scrapy.UseCases
{
    public class GetShowsUseCase : IGetShowsUseCase
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<GetShowsUseCase> _logger;

        public GetShowsUseCase(IDistributedCache cache, ILogger<GetShowsUseCase> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyCollection<Show>> ExecuteAsync(int pageIndex, int pageSize)
        {
            try
            {
                var showIndexFromCache = await _cache.GetStringAsync(Constants.CacheKeyForShowIndex);

                if (string.IsNullOrWhiteSpace(showIndexFromCache))
                {
                    _logger.CacheIsNotAvailableForShowIds();

                    return new List<Show>();
                }

                var showIndex = JsonSerializer.Deserialize<int[]>(showIndexFromCache);

                var shows = GetPaginatedShows(pageIndex, pageSize, showIndex);

                _logger.ShowsRetrievedSuccessfully(shows.Count);

                return shows;
            }
            catch (Exception ex)
            {
                _logger.FailedToGetShows(pageIndex, pageSize, ex);

                throw;
            }
        }

        private List<Show> GetPaginatedShows(int pageIndex, int pageSize, int[] showIndex) =>
            showIndex
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(async id => await GetCastAsync(id))
                .Select(task => task.Result)
                .ToList();

        private async Task<Show> GetCastAsync(int id)
        {
            var castCache = await _cache.GetStringAsync(Constants.GetCacheKeyForShow(id));

            if (string.IsNullOrWhiteSpace(castCache))
            {
                _logger.CastInfoIsNotAvailable(id);

                return new Show { Id = id, Name = "Cast info is not yet available", Cast = new List<Cast>() };
            }

            return JsonSerializer.Deserialize<Show>(castCache);
        }
    }
}
