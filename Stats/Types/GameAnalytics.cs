using LanPlayServer.Utils;
using System.Collections.Generic;

namespace LanPlayServer.Stats.Types
{
    public class GameAnalytics
    {
        public string       Id             { get; set; }
        public int          PlayerCount    { get; set; }
        public int          MaxPlayerCount { get; set; }
        public string       GameName       { get; set; }
        public string       AppId          { get; set; }
        public string       AppVersion     { get; set; }
        public string       Mode           { get; set; }
        public string       Status         { get; set; }
        public int          SceneId        { get; set; }
        public List<string> Players        { get; set; }

        public static GameAnalytics FromGame(HostedGame game)
        {
            ulong appId = (ulong)game.Info.NetworkId.IntentId.LocalCommunicationId;
            string gameName = GameList.GetGameById(appId)?.Name ?? "Unknown";
            var players = new List<string>();

            foreach (var player in game.Info.Ldn.Nodes.AsSpan()[..game.Info.Ldn.NodeCount])
            {
                string name = StringUtils.ReadUtf8String(player.UserName.AsSpan());

                // Would like to add more player information here, but that needs a bit more work.
                players.Add(name);
            }

            return new()
            {
                Id = game.Id,
                PlayerCount = game.Info.Ldn.NodeCount,
                MaxPlayerCount = game.Info.Ldn.NodeCountMax,
                GameName = gameName,
                AppId = appId.ToString("x16"),
                AppVersion = game.GameVersion,
                Mode = game.IsP2P ? "P2P" : "Master Server Proxy",
                Status = game.Info.Ldn.StationAcceptPolicy == 1 ? "Joinable" : "Not Joinable",
                SceneId = game.Info.NetworkId.IntentId.SceneId,
                Players = players
            };
        }
    }
}