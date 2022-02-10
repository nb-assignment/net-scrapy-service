using Scrapy.Models;

namespace Scrapy.Adapters.TvMaze.Models
{
    public class CastResponse
    {
        public PersonResponse Person { get; set; }

        public Cast ToDomain() => new() { Person = Person.ToDomain() };
    }
}