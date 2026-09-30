using System;
using System.IO;
using System.Reflection;
using WizshBoneTwitchIntegration.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// State/logic behind the News dialog: loads the embedded resources/news.yaml and tracks, in
    /// an install-wide file next to viewers.yaml (<see cref="WizshBoneTwitchIntegration.newsSeenPath"/>),
    /// the highest news version the player has already closed. News is only "unseen" when its
    /// version is strictly higher than the saved one.
    /// </summary>
    internal static class NewsHelper
    {
        private const string NewsResourceName = "WizshBoneTwitchIntegration.resources.news.yaml";

        private class NewsSeenData
        {
            public int version;
        }

        private static NewsData s_news;
        private static bool s_loadAttempted;

        /// <summary>
        /// The embedded news, loaded once and cached. Null if the resource is missing or
        /// unparsable (logged) - callers treat that as "no news".
        /// </summary>
        public static NewsData LoadNews()
        {
            if (s_loadAttempted)
                return s_news;

            s_loadAttempted = true;

            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NewsResourceName))
                {
                    if (stream == null)
                    {
                        Jotunn.Logger.LogWarning($"[WBTI] Embedded news resource '{NewsResourceName}' not found.");
                        return null;
                    }

                    using (StreamReader reader = new StreamReader(stream))
                        s_news = CreateDeserializer().Deserialize<NewsData>(reader);
                }

                if (s_news == null || s_news.sections == null)
                {
                    Jotunn.Logger.LogWarning("[WBTI] Embedded news resource is empty or has no sections.");
                    s_news = null;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Failed to load news: {e}");
                s_news = null;
            }

            return s_news;
        }

        /// <summary>
        /// True only when there is news and its version is higher than the last one closed.
        /// </summary>
        public static bool TryGetUnseenNews(out NewsData news)
        {
            news = LoadNews();
            return news != null && news.version > ReadSeenVersion();
        }

        /// <summary>
        /// Records <paramref name="news"/> as seen - a no-op unless its version is higher than the
        /// saved one, so reopening old news never changes anything.
        /// </summary>
        public static void MarkSeen(NewsData news)
        {
            if (news == null || news.version <= ReadSeenVersion())
                return;

            WriteSeenVersion(news.version);
        }

        /// <summary>
        /// Saved version, or 0 (news shows) if the file is missing or unreadable.
        /// </summary>
        private static int ReadSeenVersion()
        {
            try
            {
                string path = WizshBoneTwitchIntegration.newsSeenPath;
                if (!File.Exists(path))
                    return 0;

                using (StreamReader reader = new StreamReader(path))
                    return CreateDeserializer().Deserialize<NewsSeenData>(reader)?.version ?? 0;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not read the seen-news version, treating as unseen: {e.Message}");
                return 0;
            }
        }

        private static void WriteSeenVersion(int version)
        {
            try
            {
                string path = WizshBoneTwitchIntegration.newsSeenPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                ISerializer serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                File.WriteAllText(path, serializer.Serialize(new NewsSeenData { version = version }));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Could not save the seen-news version: {e}");
            }
        }

        private static IDeserializer CreateDeserializer()
        {
            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }
    }
}
