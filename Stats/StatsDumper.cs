using LanPlayServer.Stats.Types;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System.ComponentModel;
using System.Net;

namespace LanPlayServer.Stats
{
    internal static class StatsDumper
    {
        private static ConnectionMultiplexer _redisConnection;
        private static IDatabase _db;

        public static void Start(EndPoint redisEndpoint)
        {
            _redisConnection = ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                ClientName = "LdnServer",
                EndPoints = new()
                {
                    redisEndpoint
                }
            });

            _db = _redisConnection.GetDatabase();

            IJsonCommands json = _db.JSON();

            json.Set("ldn", "$", new LdnAnalytics());
            json.Set("games", "$", new {});

            Statistics.GameAnalyticsChanged += OnGameAnalyticsChanged;
            Statistics.LdnAnalyticsChanged += OnLdnAnalyticsPropertyChanged;
        }

        public static void Stop()
        {
            Statistics.GameAnalyticsChanged -= OnGameAnalyticsChanged;
            Statistics.LdnAnalyticsChanged -= OnLdnAnalyticsPropertyChanged;

            _redisConnection.Close();
            _redisConnection.Dispose();
        }

        private static void OnLdnAnalyticsPropertyChanged(LdnAnalytics analytics)
        {
            IJsonCommands json = _db.JSON();

            json.Set("ldn", "$", analytics);
        }

        private static void OnGameAnalyticsChanged(GameAnalytics analytics, bool created)
        {
            IJsonCommands json = _db.JSON();

            if (created)
            {
                json.Set("games", $"$.{analytics.Id}", analytics);
                analytics.PropertyChanged += OnGameAnalyticsPropertyChanged;
            }
            else
            {
                analytics.PropertyChanged -= OnGameAnalyticsPropertyChanged;
                json.Del("games", $"$.{analytics.Id}");
            }
        }

        private static void OnGameAnalyticsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GameAnalytics analytics = sender as GameAnalytics;
            IJsonCommands json = _db.JSON();

            if (analytics == null)
            {
                return;
            }

            // This could be optimized to only update the changed property.
            json.Set("games", $"$.{analytics.Id}", analytics);
        }
    }
}