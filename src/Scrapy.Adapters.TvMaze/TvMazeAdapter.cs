using Microsoft.Extensions.DependencyInjection;
using Refit;
using Scrapy.Adapters.TvMaze.Configurations;
using Scrapy.Adapters.TvMaze.Infrastructure.Refit;
using Scrapy.Adapters.TvMaze.Ports;
using Scrapy.Ports;

namespace Scrapy.Adapters.TvMaze
{
    public static class TvMazeAdapter
    {
        public static IHttpClientBuilder AddTvMazeAdapter(this IServiceCollection serviceCollection, TvMazeSettings tvMazeSettings)
        {
            _ = tvMazeSettings ?? throw new ArgumentNullException(nameof(tvMazeSettings));

            serviceCollection.AddSingleton(tvMazeSettings);

            serviceCollection.AddScoped<ITvMazeStore, TvMazeStore>();

            return serviceCollection.AddRefitClient<ITvMazeApi>().ConfigureTvMazeApi(tvMazeSettings);
        }

        private static IHttpClientBuilder ConfigureTvMazeApi(this IHttpClientBuilder hcb, TvMazeSettings settings)
            => hcb.ConfigureHttpClient(c => c.BaseAddress = new Uri(settings.ApiUrl));
    }
}
