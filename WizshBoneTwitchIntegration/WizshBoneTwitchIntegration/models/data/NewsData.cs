using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    /// <summary>
    /// Shape of the embedded resources/news.yaml - see <see cref="Helpers.NewsHelper"/>.
    /// </summary>
    internal class NewsData
    {
        public int version;
        public string title;
        public List<NewsSection> sections;
    }

    internal class NewsSection
    {
        public string heading;
        public string body;
    }
}
