using Scrapy.Models;

namespace Scrapy.Adapters.TvMaze.Models
{
    public class PersonResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Birthday { get; set; }

        public Person ToDomain() => new() { Id = Id, Name = Name, Birthday = Birthday };
    }
}