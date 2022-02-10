using Microsoft.Extensions.Logging;

namespace Scrapy.Jobs.Extentions
{
    public static class Logger
    {
        public static void ServiceStarted(this ILogger logger, string serviceName) =>
            logger.LogInformation($"Started {serviceName} at {DateTime.UtcNow}.");

        public static void ServiceStopped(this ILogger logger, string serviceName) =>
            logger.LogInformation($"Stopped {serviceName} at {DateTime.UtcNow}.");

        public static void ServiceFailed(this ILogger logger, Exception exception) =>
            logger.LogError($"Scheduled Service failed due to {exception.Message}. Inner exception: {exception.InnerException}");

        public static void ScrappingStarted(this ILogger logger, string serviceName) =>
           logger.LogError($"Starting Service {serviceName} for scrapping and storing the shows along with casts.");

        public static void ScrappingFailed(this ILogger logger, Exception exception) =>
            logger.LogError($"Scrapping failed due to {exception.Message}. Inner exception: {exception.InnerException}");
    }
}
