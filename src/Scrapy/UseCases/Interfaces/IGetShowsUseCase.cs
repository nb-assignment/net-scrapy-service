using Scrapy.Models;

namespace Scrapy.UseCases.Interfaces
{
    public interface IGetShowsUseCase
    {
        /// <summary>
        /// Gets the shows and casts from distributed cache on basis of page index and page size
        /// </summary>
        Task<IReadOnlyCollection<Show>> ExecuteAsync(int pageIndex, int pageSize);
    }
}
