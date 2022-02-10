using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Scrapy.Jobs.Services;
using Scrapy.Models;
using Scrapy.Ports;
using Scrapy.Utility.Constants;
using Xunit;

namespace Scrapy.Jobs.Tests.Services
{
    public class ScrapperServiceTests
    {
        private readonly Mock<ITvMazeStore> _tvMazeStoreMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<ScrapperService>> _loggerMock;
        private ScrapperService _scrapperService;

        public ScrapperServiceTests()
        {
            _tvMazeStoreMock = new Mock<ITvMazeStore>();
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<ScrapperService>>();
            _scrapperService = new ScrapperService(_tvMazeStoreMock.Object, _cacheMock.Object, _loggerMock.Object);
        }

        [Theory, AutoData]
        internal async Task GivenScrapperService_WhenStarts_CacheForShowIdsAndIndividualShowIsFilled(IEnumerable<Show> shows, CancellationToken cancellationToken)
        {
            var options = Options.Create(new MemoryDistributedCacheOptions());
            IDistributedCache distributedCache = new MemoryDistributedCache(options);

            _scrapperService = new ScrapperService(_tvMazeStoreMock.Object, distributedCache, _loggerMock.Object);

            _tvMazeStoreMock.Setup(_tvMazeStoreMock => _tvMazeStoreMock.GetShowsAsync(0, cancellationToken)).ReturnsAsync(shows);

            await _scrapperService.ScrapAsync(cancellationToken);

            distributedCache.Should().NotBeNull();

            var showIdsCache = await distributedCache.GetStringAsync(Constants.CacheKeyForShowIndex, cancellationToken);

            showIdsCache.Should().NotBeNull();

            var showIds = JsonSerializer.Deserialize<int[]>(showIdsCache);

            showIds.Should().BeEquivalentTo(shows.Select(s => s.Id));

            var firstShow = shows.FirstOrDefault();
            var showIndex = await distributedCache.GetStringAsync($"show-{firstShow?.Id}", cancellationToken);

            showIndex.Should().NotBeNull();

            var show = JsonSerializer.Deserialize<Show>(showIndex);

            show?.Id.Should().Be(firstShow?.Id);
        }
    }
}
