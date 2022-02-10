using Microsoft.Extensions.Logging;

namespace Scrapy.Extentions
{
    public static class Logger
    {
        public static void ShowsRetrievedSuccessfully(this ILogger logger, int showCount) =>
            logger.LogInformation($"Shows along with casts retrieved successfully from cache. Number of shows retrieved: {showCount}");

        public static void CacheIsNotAvailableForShowIds(this ILogger logger) =>
            logger.LogWarning("Data for show Ids is not available in cache");

        public static void CastInfoIsNotAvailable(this ILogger logger, int showId) =>
            logger.LogWarning($"Cast information is not yet available for showId: {showId}");

        public static void FailedToGetShows(this ILogger logger, int pageIndex, int pageSize, Exception exception) =>
            logger.LogError($"Failed to get shows for page index {pageIndex} and page size {pageSize}. Inner exception: {exception.InnerException}");
    }
}
