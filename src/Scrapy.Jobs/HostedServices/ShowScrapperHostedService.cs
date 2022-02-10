using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using NodaTime;
using Scrapy.Jobs.Extentions;
using Scrapy.Jobs.Models;
using Scrapy.Jobs.Services.Interfaces;

namespace Scrapy.Jobs.HostedServices
{
    public class ShowScrapperHostedService : HostedServiceBase<ShowScrapperHostedService>
    {
        private DateTime _nextRun;
        private DateTime _currentRun;
        private readonly CrontabSchedule _schedule;
        private readonly ScheduleConfig _scheduleConfig;
        private readonly IClock _clock;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ShowScrapperHostedService(
            ILogger<ShowScrapperHostedService> logger,
            IClock clock,
            IOptions<ScheduleConfig> config,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger)
        {
            _scheduleConfig = config.Value;
            _clock = clock;
            _serviceScopeFactory = serviceScopeFactory;

            _schedule = CrontabSchedule.Parse(_scheduleConfig.CronExpression);
            _nextRun = _clock.GetCurrentInstant().ToDateTimeUtc();
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            _currentRun = _clock.GetCurrentInstant().ToDateTimeUtc();
            using var scope = _serviceScopeFactory.CreateScope();

            _logger.ScrappingStarted(nameof(IScrapperService));

            var showScrapperService = scope.ServiceProvider.GetRequiredService<IScrapperService>();
            await showScrapperService.ScrapAsync(cancellationToken);
        }

        public override async Task AfterWorkDoneDelay()
        {
            _nextRun = _schedule.GetNextOccurrence(_currentRun);
            await Task.Delay(_nextRun - _currentRun);
        }

        public override Task OnErrorDelay() => Task.Delay(TimeSpan.FromSeconds(_scheduleConfig.OnErrorDelay));
    }
}