using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Scrapy.Adapters.TvMaze.Configurations;
using Scrapy.Adapters.TvMaze.Ports;
using Scrapy.Ports;
using Xunit;

namespace Scrapy.Adapters.TvMaze.Tests
{
    public class TvMazeAdapterTests
    {
        [Fact]
        public void GivenAContainer_WhenRegisteringTvMazeAdapter_ExpectedServicesAreRegistered()
        {
            var serviceCollection = new ServiceCollection();

            RegisterDependencies(serviceCollection);
            var tvMazeStore = serviceCollection.BuildServiceProvider().GetService(typeof(ITvMazeStore));
            tvMazeStore.Should().NotBeNull().And.BeOfType<TvMazeStore>();
        }

        [Fact]
        public void GivenAContainer_WhenRegisteringTvMazeSettings_ExpectedSettingsAreRegistered()
        {
            var serviceCollection = new ServiceCollection();

            RegisterDependencies(serviceCollection);
            var tvMazeSettings = serviceCollection.BuildServiceProvider().GetService(typeof(TvMazeSettings));
            tvMazeSettings.Should().NotBeNull();
        }

        [Fact]
        public void GivenAContainer_WhenTvMazeSettingsAreNull_ThrowsArgumentNullException()
        {
            var serviceCollection = new ServiceCollection();

            Action action = () => serviceCollection.AddTvMazeAdapter(null);
            action.Should().Throw<ArgumentNullException>();
        }

        private static void RegisterDependencies(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTvMazeAdapter(new TvMazeSettings { ApiUrl = "http://localhost/" });
        }
    }
}
