namespace Scrapy.Jobs.Services.Interfaces
{
    public interface IScrapperService
    {
        Task ScrapAsync(CancellationToken cancellationToken);
    }
}
