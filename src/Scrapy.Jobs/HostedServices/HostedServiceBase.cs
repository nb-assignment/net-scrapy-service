using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scrapy.Jobs.Extentions;

namespace Scrapy.Jobs.HostedServices
{
    public abstract class HostedServiceBase<T> : BackgroundService where T : class
    {
        protected readonly ILogger<T> _logger;

        protected HostedServiceBase(ILogger<T> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.ServiceStarted(typeof(T).Name);

            cancellationToken.Register(() => _logger.ServiceStopped(typeof(T).Name));

            do
            {
                try
                {
                    await DoWorkAsync(cancellationToken);
                    await AfterWorkDoneDelay();
                }
                catch (Exception ex)
                {
                    _logger.ServiceFailed(ex);

                    await OnErrorDelay();
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        public virtual Task OnErrorDelay() => Task.Delay(TimeSpan.FromSeconds(15));

        public virtual Task AfterWorkDoneDelay() => Task.Delay(TimeSpan.FromDays(1));

        public abstract Task DoWorkAsync(CancellationToken cancellationToken);
    }
}