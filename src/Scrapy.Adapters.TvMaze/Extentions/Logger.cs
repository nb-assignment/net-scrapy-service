using Microsoft.Extensions.Logging;

namespace Scrapy.Adapters.TvMaze.Extentions
{
    public static class Logger
    {
        public static void ShowsNotFound(this ILogger logger, int pageIndex) =>
            logger.LogWarning($"Shows are not returned from TvMaze API for page index: {pageIndex}");

        public static void CastsNotFound(this ILogger logger, int showId) =>
            logger.LogWarning($"Casts are not returned from TvMaze API for show Id: {showId}");
    }
}
