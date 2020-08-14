using NetCoreServer;
using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace LanPlayServer
{
    class LdnServer : TcpServer
    {
        public ConcurrentDictionary<string, HostedGame> HostedGames;
        public bool UseProxy => true;

        public LdnServer(IPAddress address, int port) : base(address, port)
        {
            HostedGames = new ConcurrentDictionary<string, HostedGame>();
            OptionNoDelay = true;
        }

        public HostedGame CreateGame(string id, NetworkInfo info)
        {
            HostedGame game = new HostedGame(id, info);

            HostedGames.AddOrUpdate(id, game, (id, oldGame) =>
            {
                oldGame.Close();
                return game;
            });

            return game;
        }

        public HostedGame FindGame(string id)
        {
            HostedGames.TryGetValue(id, out HostedGame result);

            return result;
        }

        public KeyValuePair<string, HostedGame>[] All()
        {
            return HostedGames.ToArray();
        }

        public int Scan(ref NetworkInfo[] info, ScanFilter filter, string passphrase)
        {
            KeyValuePair<string, HostedGame>[] all = HostedGames.ToArray();

            int results = 0;

            for (int i = 0; i < all.Length; i++)
            {
                HostedGame game = all[i].Value;

                if (game.Passphrase != passphrase)
                {
                    continue;
                }

                NetworkInfo scanInfo = game.Info;

                if (scanInfo.Ldn.StationAcceptPolicy == 1)
                {
                    // Optimization: don't tell anyone about unjoinable networks.

                    continue;
                }

                if (filter.Flag.HasFlag(ScanFilterFlag.LocalCommunicationId))
                {
                    if (scanInfo.NetworkId.IntentId.LocalCommunicationId != filter.NetworkId.IntentId.LocalCommunicationId)
                    {
                        continue;
                    }
                }

                if (filter.Flag.HasFlag(ScanFilterFlag.SceneId))
                {
                    if (scanInfo.NetworkId.IntentId.SceneId != filter.NetworkId.IntentId.SceneId)
                    {
                        continue;
                    }
                }

                if (filter.Flag.HasFlag(ScanFilterFlag.SessionId))
                {
                    if (!scanInfo.NetworkId.SessionId.SequenceEqual(filter.NetworkId.SessionId))
                    {
                        continue;
                    }
                }

                if (filter.Flag.HasFlag(ScanFilterFlag.Ssid))
                {
                    IEnumerable<byte> gameSsid = scanInfo.Common.Ssid.Name.Take(scanInfo.Common.Ssid.Length);
                    IEnumerable<byte> scanSsid = filter.Ssid.Name.Take(filter.Ssid.Length);
                    if (!gameSsid.SequenceEqual(scanSsid))
                    {
                        continue;
                    }
                }

                if (filter.Flag.HasFlag(ScanFilterFlag.NetworkType))
                {
                    if (scanInfo.Common.NetworkType != filter.NetworkType)
                    {
                        continue;
                    }
                }

                // Mac address filter not implemented, since they are currently random.

                if (results >= info.Length)
                {
                    Array.Resize(ref info, info.Length + 1);
                }

                info[results++] = scanInfo;
            }

            return results;
        }

        public void CloseGame(string id)
        {
            HostedGames.Remove(id, out HostedGame removed);
            removed?.Close();
        }

        protected override TcpSession CreateSession() { return new LdnSession(this); }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"LDN TCP server caught an error with code {error}");
        }
    }
}
