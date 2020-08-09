using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System;
using System.Collections.Generic;
using System.Net;

namespace LanPlayServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 30456;

            Console.WriteLine($"TCP server port: {port}");
            Console.WriteLine();

            var server = new LdnServer(IPAddress.Any, port);

            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine(" done!");

            for (;;)
            {
                string line = Console.ReadLine();

                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                    continue;
                }

                if (line == "close")
                {
                    break;
                }

                bool commandValid = line switch {
                    "list" => List(server),
                    _ => false
                };

                if (!commandValid)
                {
                    Console.WriteLine("Invalid command.");
                }
            }

            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine(" done!");
        }

        static bool List(LdnServer server)
        {
            KeyValuePair<string, HostedGame>[] games = server.All();

            Dictionary<string, List<HostedGame>> gamesByPassphrase = new Dictionary<string, List<HostedGame>>();

            foreach (KeyValuePair<string, HostedGame> game in games)
            {
                string passphrase = game.Value.Passphrase ?? "";

                List<HostedGame> target;
                if (!gamesByPassphrase.TryGetValue(passphrase, out target))
                {
                    target = new List<HostedGame>();

                    gamesByPassphrase.Add(passphrase, target);
                }

                target.Add(game.Value);
            }

            int gameCount = 0;
            int playerCount = 0;
            int privateGameCount = 0;
            int privatePlayerCount = 0;
            int masterProxyCount = 0;
            int inProgressCount = 0;

            foreach (KeyValuePair<string, List<HostedGame>> group in gamesByPassphrase)
            {
                bool publicGroup = group.Key == "";
                Console.WriteLine(publicGroup ? "== PUBLIC ==" : $"== Passphrase: {group.Key} ==");

                foreach (HostedGame game in group.Value)
                {
                    string id = game.Id;
                    NetworkInfo info = game.Info;

                    ulong titleId = info.NetworkId.IntentId.LocalCommunicationId;
                    string gameName = GameList.GetGameById(titleId)?.Name ?? "Unknown";
                    string titleString = titleId.ToString("x16");

                    string mode = game.IsP2P ? "P2P" : "Master Server Proxy";
                    if (!game.IsP2P)
                    {
                        masterProxyCount++;
                    }

                    string status = "";

                    gameCount++;

                    if (!publicGroup)
                    {
                        privateGameCount++;
                        privatePlayerCount += info.Ldn.NodeCount;
                    }

                    if (info.Ldn.StationAcceptPolicy == 1)
                    {
                        inProgressCount++;
                        status = ", Not Joinable";
                    }

                    Console.WriteLine($" {id} ({info.Ldn.NodeCount}/{info.Ldn.NodeCountMax}): {gameName} ({titleString}) - {mode}{status}");

                    playerCount += info.Ldn.NodeCount;

                    for (int i = 0; i < info.Ldn.NodeCount; i++)
                    {
                        NodeInfo player = info.Ldn.Nodes[i];
                        string name = StringUtils.ReadUtf8String(player.UserName);

                        // Would like to print IP here, but needs a bit more work.
                        Console.WriteLine($"  - {name}");
                    }

                    Console.WriteLine();
                }
            }

            Console.WriteLine("==========");
            Console.WriteLine("Game Summary:");
            Console.WriteLine($"{gameCount} total games. ({privateGameCount} private, {gameCount - privateGameCount} public)");
            Console.WriteLine($" {inProgressCount} games in progress (not joinable).");
            Console.WriteLine($" {masterProxyCount} games using the master server as a proxy rather than P2P.");
            Console.WriteLine("Player Summary:");
            Console.WriteLine($"{playerCount} total players. ({privatePlayerCount} in private games, {playerCount - privatePlayerCount} in public)");

            return true;
        }
    }
}