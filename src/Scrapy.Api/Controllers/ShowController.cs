using System.Net;
using Microsoft.AspNetCore.Mvc;
using Scrapy.UseCases.Interfaces;

namespace Scrapy.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShowsController : ControllerBase
    {
        private readonly IGetShowsUseCase _getShowsUseCase;

        public ShowsController(IGetShowsUseCase getShowsUseCase)
        {
            _getShowsUseCase = getShowsUseCase ?? throw new ArgumentNullException(nameof(getShowsUseCase));
        }

        /// <summary>
        /// API endpoint to get shows including the cast information
        /// </summary>
        /// <param name="pageSize">Number of records required in one single page</param>
        /// <param name="pageIndex">Index of the page</param>
        /// <returns>List of shows and list of casts for each show</returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            if (pageSize is < 0 or > 240)
            {
                return BadRequest("page size should be between 0 to 240");
            }

            if (pageIndex < 0)
            {
                return BadRequest("page size can not be lower than 0");
            }

            var result = await _getShowsUseCase.ExecuteAsync(pageIndex, pageSize);

            return Ok(result);
        }
    }
}
