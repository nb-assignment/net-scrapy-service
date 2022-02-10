using Microsoft.Extensions.Logging;
using Refit;
using Scrapy.Adapters.TvMaze.Extentions;
using Scrapy.Adapters.TvMaze.Infrastructure.Refit;
using Scrapy.Exceptions;
using Scrapy.Models;
using Scrapy.Ports;

namespace Scrapy.Adapters.TvMaze.Ports
{
    public class TvMazeStore : ITvMazeStore
    {
        private readonly ITvMazeApi _tvMazeApi;
        private readonly ILogger<TvMazeStore> _logger;

        public TvMazeStore(ITvMazeApi tvMazeApi, ILogger<TvMazeStore> logger)
        {
            _tvMazeApi = tvMazeApi ?? throw new ArgumentNullException(nameof(tvMazeApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Show>> GetShowsAsync(int page, CancellationToken cancellationToken)
        {
            try
            {
                var shows = await _tvMazeApi.GetShowsAsync(page, cancellationToken);

                if (shows is null || !shows.Any())
                {
                    _logger.ShowsNotFound(page);

                    return null;
                };

                return shows.Select(s => s.ToDomain());
            }
            catch (ApiException ex)
            {
                throw new GetShowsExceptions(ex.Message);
            }
        }

        public async Task<IEnumerable<Cast>> GetCastAsync(int showId, CancellationToken cancellationToken)
        {
            try
            {
                var casts = await _tvMazeApi.GetCastAsync(showId, cancellationToken);

                if (casts is null || !casts.Any())
                {
                    _logger.CastsNotFound(showId);

                    return null;
                };

                return casts.OrderByDescending(x => x.Person.Birthday).Select(cast => cast.ToDomain());
            }
            catch (ApiException ex)
            {
                throw new GetCastsExceptions(ex.Message);
            }
        }
    }
}
