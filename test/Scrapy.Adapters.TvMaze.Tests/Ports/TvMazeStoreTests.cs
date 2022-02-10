using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Scrapy.Adapters.TvMaze.Infrastructure.Refit;
using Scrapy.Adapters.TvMaze.Models;
using Scrapy.Adapters.TvMaze.Ports;
using Scrapy.Exceptions;
using Xunit;

namespace Scrapy.Adapters.TvMaze.Tests.Ports
{
    public class TvMazeStoreTests
    {
        private readonly Mock<ILogger<TvMazeStore>> _loggerMock;
        private readonly Mock<ITvMazeApi> _mockTvMazeApi;
        private readonly TvMazeStore _tvMazeStore;

        public TvMazeStoreTests()
        {
            _loggerMock = new Mock<ILogger<TvMazeStore>>();
            _mockTvMazeApi = new Mock<ITvMazeApi>();
            _tvMazeStore = new TvMazeStore(_mockTvMazeApi.Object, _loggerMock.Object);
        }

        [Theory, AutoData]
        internal void GivenExceptionIsReturnedWhileCallingApi_WhenGettingShowsFromStore_GetShowsExceptionsIsThrown(int page, Exception exception, CancellationToken cancellationToken)
        {
            _mockTvMazeApi.Setup(s => s.GetShowsAsync(page, cancellationToken)).ThrowsAsync(exception);

            Func<Task> call = async () => await _tvMazeStore.GetShowsAsync(page, cancellationToken);

            call.Should().ThrowExactlyAsync<GetShowsExceptions>();
        }

        [Theory, AutoData]
        internal void GivenShowsAreNotReturnedFromApi_WhenGettingShowsFromStore_GetShowsExceptionsIsThrown(int page, CancellationToken cancellationToken)
        {
            _mockTvMazeApi.Setup(s => s.GetShowsAsync(page, cancellationToken)).ReturnsAsync(new List<ShowResponse>());

            Func<Task> call = async () => await _tvMazeStore.GetShowsAsync(page, cancellationToken);

            call.Should().ThrowExactlyAsync<GetShowsExceptions>();
        }

        [Theory, AutoData]
        internal async Task GivenShowsAreReturnedFromApi_WhenGettingShowsFromStore_ExpectedShowsAreReturned(int page, IEnumerable<ShowResponse> shows, CancellationToken cancellationToken)
        {
            _mockTvMazeApi.Setup(s => s.GetShowsAsync(page, cancellationToken)).ReturnsAsync(shows);

            var result = await _tvMazeStore.GetShowsAsync(page, cancellationToken);

            result.Should().NotBeNull();

            result.Should().BeEquivalentTo(shows);
        }

        [Theory, AutoData]
        internal void GivenExceptionIsReturnedWhileCallingApi_WhenGettingCastsFromStore_GetCastsExceptionsIsThrown(int showId, Exception exception, CancellationToken cancellationToken)
        {
            _mockTvMazeApi.Setup(s => s.GetCastAsync(showId, cancellationToken)).ThrowsAsync(exception);

            Func<Task> call = async () => await _tvMazeStore.GetCastAsync(showId, cancellationToken);

            call.Should().ThrowExactlyAsync<GetCastsExceptions>();
        }

        [Theory, AutoData]
        internal void GivenCastsAreNotReturnedFromApi_WhenGettingCastsFromStore_GetShowsExceptionsIsThrown(int page, CancellationToken cancellationToken)
        {
            _mockTvMazeApi.Setup(s => s.GetCastAsync(page, cancellationToken)).ReturnsAsync(new List<CastResponse>());

            Func<Task> call = async () => await _tvMazeStore.GetShowsAsync(page, cancellationToken);

            call.Should().ThrowExactlyAsync<GetCastsExceptions>();
        }

        [Theory, AutoData]
        internal async Task GivenCastsAreReturnedFromApi_WhenGettingCastsFromStore_ExpectedCastsAreReturned(int showId, IEnumerable<CastResponse> casts, CancellationToken cancellationToken)
        {
            _mockTvMazeApi.Setup(s => s.GetCastAsync(showId, cancellationToken)).ReturnsAsync(casts);

            var result = await _tvMazeStore.GetCastAsync(showId, cancellationToken);

            result.Should().NotBeNull();

            result.Should().BeEquivalentTo(casts);
        }

        [Theory, AutoData]
        internal async Task GivenCastsAreReturnedFromApi_WhenGettingCastsFromStore_CastIsOrderedByBirthdayDesc(int showId, CancellationToken cancellationToken)
        {
            var casts = new List<CastResponse>()
            {
                new CastResponse
                {
                    Person = new PersonResponse
                    {
                        Id = 1,
                        Name = "test",
                        Birthday = "2020-02-10"
                    }
                },
                new CastResponse
                {
                    Person = new PersonResponse
                    {
                        Id = 1,
                        Name = "test",
                        Birthday = "2021-02-10"
                    }
                }
            };

            _mockTvMazeApi.Setup(s => s.GetCastAsync(showId, cancellationToken)).ReturnsAsync(casts);

            var result = await _tvMazeStore.GetCastAsync(showId, cancellationToken);

            result.Should().NotBeNull();

            result.FirstOrDefault()?.Person.Birthday.Should().Be("2021-02-10");
        }
    }
}
