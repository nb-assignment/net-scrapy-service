using Refit;
using Scrapy.Adapters.TvMaze.Models;

namespace Scrapy.Adapters.TvMaze.Infrastructure.Refit
{
    public interface ITvMazeApi
    {
        [Get("/shows")]
        Task<IEnumerable<ShowResponse>> GetShowsAsync(int page, CancellationToken cancellationToken);

        [Get("/shows/{id}/cast")]
        Task<IEnumerable<CastResponse>> GetCastAsync(int id, CancellationToken cancellationToken);
    }
}
