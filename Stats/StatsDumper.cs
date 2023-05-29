using System.Collections.Generic;
using System.Linq;
using LanPlayServer.Stats.Types;
using LanPlayServer.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LanPlayServer.Stats
{
    internal static class StatsDumper
    {
        private static readonly LdnAnalyticsSerializerContext LdnAnalyticsJsonContext = new(JsonHelper.GetDefaultSerializerOptions());

        private static readonly GameAnalyticsSerializerContext GameAnalyticsJsonContext = new(JsonHelper.GetDefaultSerializerOptions());

        public static async Task WriteJsonFiles(IReadOnlyCollection<HostedGame> games, string outputDirectory, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                return;
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            foreach (StatsType statsType in Enum.GetValuesAsUnderlyingType<StatsType>())
            {
                string filePath = Path.Combine(outputDirectory, $"{statsType.ToString().ToLower()}.json");
                await File.WriteAllTextAsync(filePath, GetJsonArray(games, statsType), cancellationToken);
            }
        }

        private static Dictionary<string, List<HostedGame>> GetGamesByPassphrase(IEnumerable<HostedGame> games)
        {
            // All hosted games.
            Dictionary<string, List<HostedGame>> gamesByPassphrase = new();

            foreach (HostedGame game in games)
            {
                if (game.TestReadLock())
                {
                    continue;
                }

                string passphrase = game.Passphrase ?? "";

                if (!gamesByPassphrase.TryGetValue(passphrase, out List<HostedGame> target))
                {
                    target = new List<HostedGame>();

                    gamesByPassphrase.Add(passphrase, target);
                }

                target.Add(game);
            }

            return gamesByPassphrase;
        }

        public static string GetJsonArray(IEnumerable<HostedGame> games, StatsType type, string gameAppId = "")
        {
            var gamesByPassphrase = GetGamesByPassphrase(games);

            int totalGameCount     = 0;
            int totalPlayerCount   = 0;
            int privateGameCount   = 0;
            int privatePlayerCount = 0;
            int masterProxyCount   = 0;
            int inProgressCount    = 0;

            List<GameAnalytics> gameAnalytics = new();

            foreach ((string passphrase, List<HostedGame> group) in gamesByPassphrase)
            {
                bool isGamePublic = string.IsNullOrWhiteSpace(passphrase);

                foreach (HostedGame game in group)
                {
                    if (game.Info.Ldn.NodeCount == 0)
                    {
                        continue;
                    }

                    GameAnalytics analytics = GameAnalytics.FromGame(game);

                    if (!game.IsP2P)
                    {
                        masterProxyCount++;
                    }

                    totalGameCount++;

                    if (!isGamePublic)
                    {
                        privateGameCount++;
                        privatePlayerCount += game.Info.Ldn.NodeCount;
                    }

                    if (game.Info.Ldn.StationAcceptPolicy == 1)
                    {
                        inProgressCount++;
                    }

                    totalPlayerCount += game.Info.Ldn.NodeCount;

                    if (isGamePublic)
                    {
                        gameAnalytics.Add(analytics);
                    }
                }
            }

            switch (type)
            {
                case StatsType.Game:
                    LdnAnalytics ldnAnalytics = new()
                    {
                        TotalGamesCount     = totalGameCount,
                        PrivateGamesCount   = privateGameCount,
                        PublicGamesCount    = totalGameCount - privateGameCount,
                        InProgressCount     = inProgressCount,
                        MasterProxyCount    = masterProxyCount,
                        TotalPlayersCount   = totalPlayerCount,
                        PrivatePlayersCount = privatePlayerCount,
                        PublicPlayersCount  = totalPlayerCount - privatePlayerCount
                    };

                    return JsonHelper.Serialize(ldnAnalytics, LdnAnalyticsJsonContext.LdnAnalytics);

                case StatsType.Ldn:
                    return JsonHelper.Serialize(
                        string.IsNullOrWhiteSpace(gameAppId) ?
                            gameAnalytics :
                            gameAnalytics.Where(game => String.Equals(game.AppId, gameAppId, StringComparison.InvariantCultureIgnoreCase)).ToList(),
                        GameAnalyticsJsonContext.ListGameAnalytics
                        );

                default:
                    throw new NotImplementedException();
            }
        }
    }
}