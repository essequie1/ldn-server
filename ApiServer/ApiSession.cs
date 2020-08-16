using NetCoreServer;
using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace LanPlayServer
{
    class ApiSession : HttpSession
    {
        readonly LdnServer _ldnServer;

        public ApiSession(HttpServer server, LdnServer ldnServer) : base(server)
        {
            _ldnServer = ldnServer;
        }

        protected override void OnReceivedRequest(HttpRequest request)
        {
            string bodyResponse = "";

            string key = Uri.UnescapeDataString(request.Url);

            if (request.Method == "GET")
            {
                if (key == ApiEndpoints.Api || key == ApiEndpoints.Games)
                {
                    bodyResponse = List(key);
                }
                else
                {
                    key = key.Replace(ApiEndpoints.Games + "?titleid=", "", StringComparison.InvariantCultureIgnoreCase);

                    bool isTitleId = Regex.IsMatch(key, "^[a-z0-9/._-]{16}$");
                    if (isTitleId)
                    {
                        bodyResponse = List(ApiEndpoints.Games, key);
                    }
                }
            }

            HttpResponse httpResponse = new HttpResponse();
            httpResponse.Clear();

            if (bodyResponse != "")
            {
                httpResponse.SetBegin(200);
                httpResponse.SetContentType(".json");
                httpResponse.SetHeader("Access-Control-Allow-Origin", "*");
                httpResponse.SetBody(bodyResponse);
            }
            else
            {
                string[] fileList = { "/", "/index.html", "/style.css", "/main.js" };

                if (fileList.Contains(key))
                {
                    httpResponse.SetBegin(200);
                    httpResponse.SetHeader("Cache-Control", $"max-age={TimeSpan.FromHours(1).Seconds}");

                    if ((key == fileList[0]) || (key == fileList[1]))
                    {
                        httpResponse.SetContentType(".html");
                        httpResponse.SetBody(File.ReadAllText($"www{fileList[1]}"));
                    }
                    else
                    {
                        httpResponse.SetContentType(Path.GetExtension(key));
                        httpResponse.SetBody(File.ReadAllText($"www{key}"));
                    }
                }
                else
                {
                    httpResponse.SetBegin(404);
                    httpResponse.SetBody("");
                }
            }

            SendResponseAsync(httpResponse);
        }

        private string List(string endpoint, string gameTitleId = "")
        {
            // List all hosted games.
            KeyValuePair<string, HostedGame>[] games = _ldnServer.All();

            // List all private hosted games.
            Dictionary<string, List<HostedGame>> gamesByPassphrase = new Dictionary<string, List<HostedGame>>();

            foreach (KeyValuePair<string, HostedGame> game in games)
            {
                string passphrase = game.Value.Passphrase ?? "";

                if (!gamesByPassphrase.TryGetValue(passphrase, out List<HostedGame> target))
                {
                    target = new List<HostedGame>();

                    gamesByPassphrase.Add(passphrase, target);
                }

                target.Add(game.Value);
            }

            int totalGameCount     = 0;
            int totalPlayerCount   = 0;
            int privateGameCount   = 0;
            int privatePlayerCount = 0;
            int masterProxyCount   = 0;
            int inProgressCount    = 0;

            List<GameAnalytics> gamesAnalytics = new List<GameAnalytics>();

            foreach (KeyValuePair<string, List<HostedGame>> group in gamesByPassphrase)
            {
                bool isGamePublic = group.Key == "";

                foreach (HostedGame game in group.Value)
                {
                    GameAnalytics gameAnalytics = new GameAnalytics();

                    string      id          = game.Id;
                    NetworkInfo info        = game.Info;
                    ulong       titleId     = info.NetworkId.IntentId.LocalCommunicationId;
                    string      gameName    = GameList.GetGameById(titleId)?.Name ?? "Unknown";
                    string      titleString = titleId.ToString("x16");

                    gameAnalytics.Mode = game.IsP2P ? "P2P" : "Master Server Proxy";
                    if (!game.IsP2P)
                    {
                        masterProxyCount++;
                    }

                    gameAnalytics.Status = "Joinable";

                    totalGameCount++;

                    if (!isGamePublic)
                    {
                        privateGameCount++;
                        privatePlayerCount += info.Ldn.NodeCount;
                    }

                    if (info.Ldn.StationAcceptPolicy == 1)
                    {
                        inProgressCount++;
                        gameAnalytics.Status = "Not Joinable";
                    }

                    gameAnalytics.Id             = id;
                    gameAnalytics.PlayerCount    = info.Ldn.NodeCount;
                    gameAnalytics.MaxPlayerCount = info.Ldn.NodeCountMax;
                    gameAnalytics.GameName       = gameName;
                    gameAnalytics.TitleId        = titleString;

                    totalPlayerCount += info.Ldn.NodeCount;

                    gameAnalytics.Players = new List<string>();

                    for (int i = 0; i < info.Ldn.NodeCount; i++)
                    {
                        NodeInfo player = info.Ldn.Nodes[i];
                        string   name   = StringUtils.ReadUtf8String(player.UserName);

                        // Would like add more players informations here, but needs a bit more work.
                        gameAnalytics.Players.Add(name);
                    }

                    if (isGamePublic)
                    {
                        gamesAnalytics.Add(gameAnalytics);
                    }
                }
            }

            if (endpoint == ApiEndpoints.Api)
            {
                LdnAnalytics ldnAnalytics = new LdnAnalytics()
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

                return JsonSerializerHelper.Serialize(ldnAnalytics);
            }
            else if (endpoint == ApiEndpoints.Games)
            {
                // return all games
                if (gameTitleId == "")
                {
                    return JsonSerializerHelper.Serialize(gamesAnalytics);
                }
                else
                {
                    return JsonSerializerHelper.Serialize(gamesAnalytics.Where(game => game.TitleId == gameTitleId).ToList());
                }
            }
            else
            {
                return "";
            }
        }

        protected override void OnReceivedRequestError(HttpRequest request, string error)
        {
            Console.WriteLine($"Request error: {error}");
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"HTTP session caught an error: {error}");
        }
    }
}