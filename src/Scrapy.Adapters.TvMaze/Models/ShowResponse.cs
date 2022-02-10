using Scrapy.Models;

namespace Scrapy.Adapters.TvMaze.Models
{
    public class ShowResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<CastResponse> Cast { get; set; }

        public Show ToDomain() => new() { Id = Id, Name = Name, Cast = Cast?.Select(c => c.ToDomain()).ToList() };
    }
}