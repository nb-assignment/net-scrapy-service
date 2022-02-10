using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Scrapy.Api.Controllers;
using Scrapy.Models;
using Scrapy.UseCases.Interfaces;
using Xunit;

namespace Scrapy.Api.Tests.Controllers
{
    public class ShowsControllerTests
    {
        private readonly ShowsController _showController;
        private readonly Mock<IGetShowsUseCase> _getShowsUseCaseMock;

        public ShowsControllerTests()
        {
            _getShowsUseCaseMock = new Mock<IGetShowsUseCase>();

            _showController = new ShowsController(_getShowsUseCaseMock.Object);
        }

        [Theory, AutoData]
        internal async Task GivenUseCaseReturnsListOfShows_WhenGettingShowsFromController_ShowsAreReturned(IReadOnlyCollection<Show> shows)
        {
            _getShowsUseCaseMock.Setup(ssm => ssm.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(shows);

            var result = await _showController.Get(1, 20);

            Assert.IsType<OkObjectResult>(result);
            var okObjectResult = (OkObjectResult)result;

            okObjectResult.Value.Should().BeEquivalentTo(shows);
        }

        [Theory]
        [InlineAutoData(241)]
        [InlineAutoData(-1)]
        internal async Task GivenPageSizeMoreThanAllowedSize_WhenGettingShowsFromController_BadRequestStatusIsReturned(int pageSize, List<Show> shows)
        {
            _getShowsUseCaseMock.Setup(s => s.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(shows);

            var result = await _showController.Get(pageSize, 20);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestObjectResult = (BadRequestObjectResult)result;

            badRequestObjectResult.Value.Should().Be("page size should be between 0 to 240");
        }

        [Theory, AutoData]
        internal async Task GivenPageIndexOutsideOfAllowedIndex_WhenGettingShowsFromController_BadRequestStatusIsReturned(IReadOnlyCollection<Show> shows)
        {
            _getShowsUseCaseMock.Setup(s => s.ExecuteAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(shows);

            var result = await _showController.Get(1, -1);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestObjectResult = (BadRequestObjectResult)result;

            badRequestObjectResult.Value.Should().Be("page size can not be lower than 0");
        }
    }
}
