using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Scrapy.Models;
using Scrapy.UseCases;
using Scrapy.Utility.Constants;
using Xunit;

namespace Scrapy.Tests.UseCases
{
    public class GetShowsUseCaseTests
    {
        private readonly Mock<ILogger<GetShowsUseCase>> _loggerMock;

        private GetShowsUseCase? _getShowsUseCase;

        public GetShowsUseCaseTests()
        {
            _loggerMock = new Mock<ILogger<GetShowsUseCase>>();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        internal async Task GivenPageIndexAndPageSize_WhenGettingShows_ShowCountIsEquivalentToSize(int pageIndex, int pageSize)
        {
            var options = Options.Create(new MemoryDistributedCacheOptions());
            IDistributedCache distributedCache = new MemoryDistributedCache(options);
            distributedCache.Set(Constants.CacheKeyForShowIndex, Encoding.ASCII.GetBytes(GetCacheIndex()));
            distributedCache.Set("show-2", Encoding.ASCII.GetBytes(GetCastFromCache()));

            _getShowsUseCase = new GetShowsUseCase(distributedCache, _loggerMock.Object);

            var shows = await _getShowsUseCase.ExecuteAsync(pageIndex, pageSize);

            shows.Should().NotBeNull();
            shows.Count.Should().Be(pageSize);
        }

        [Fact]
        internal async Task GivenCacheReturnsEmptyResults_WhenGettingShows_EmptyCollectionIsReturned()
        {
            var options = Options.Create(new MemoryDistributedCacheOptions());
            IDistributedCache distributedCache = new MemoryDistributedCache(options);

            _getShowsUseCase = new GetShowsUseCase(distributedCache, _loggerMock.Object);

            var shows = await _getShowsUseCase.ExecuteAsync(1, 1);

            shows.Count.Should().Be(0);
        }

        [Fact]
        internal async Task GivenCastCacheIsNotStored_WhenGettingShows_ShowIsReturnedWithoutCast()
        {
            var options = Options.Create(new MemoryDistributedCacheOptions());
            IDistributedCache distributedCache = new MemoryDistributedCache(options);
            distributedCache.Set(Constants.CacheKeyForShowIndex, Encoding.ASCII.GetBytes(GetCacheIndex()));

            _getShowsUseCase = new GetShowsUseCase(distributedCache, _loggerMock.Object);

            var shows = await _getShowsUseCase.ExecuteAsync(1, 1);

            shows.Should().NotBeNull();
            shows.Count.Should().Be(1);
            shows.FirstOrDefault()?.Cast.Should().BeEmpty();
        }

        private static string GetCacheIndex() => "[1, 2, 3, 4, 5]";

        private static string GetCastFromCache()
        {
            var show = new Show()
            {
                Id = 2,
                Name = "test",
                Cast = new Cast[]
                {
                    new Cast
                    {
                        Person = new Person
                        {
                            Id = 1,
                            Name = "person_1",
                            Birthday = DateTime.Now.AddDays(-10).ToString(),
                        }
                    },
                    new Cast
                    {
                        Person = new Person
                        {
                            Id = 2,
                            Name = "person_2",
                            Birthday = DateTime.Now.AddDays(-20).ToString(),
                        }
                    },
                }
            };

            return JsonSerializer.Serialize(show);
        }
    }
}
