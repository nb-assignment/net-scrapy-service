namespace Scrapy.Utility.Constants
{
    public static class Constants
    {
        public const string CacheKeyForShowIndex = "showIds";

        public static string GetCacheKeyForShow(int id) => $"show-{id}";
    }
}
