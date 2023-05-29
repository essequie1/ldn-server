using NetCoreServer;
using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LanPlayServer
{
    public class LdnServer : TcpServer
    {
        public static readonly int InactivityPingFrequency = 10000;

        public ConcurrentDictionary<string, HostedGame> HostedGames = new ConcurrentDictionary<string, HostedGame>();
        public MacAddressMemory MacAddresses { get; } = new MacAddressMemory();
        public bool UseProxy => true;

        private CancellationTokenSource _cancel = new CancellationTokenSource();

        public LdnServer(IPAddress address, int port) : base(address, port)
        {
            OptionNoDelay = true;

            Task.Run(BackgroundPingTask);
        }

        public HostedGame CreateGame(string id, NetworkInfo info, AddressList dhcpConfig, string oldOwnerID)
        {
            id = id.ToLower();
            HostedGame game = new HostedGame(id, info, dhcpConfig);
            bool idTaken = false;

            HostedGames.AddOrUpdate(id, game, (id, oldGame) =>
            {
                if (oldGame.OwnerId == oldOwnerID)
                {
                    oldGame.Close();

                    return game;
                }
                else
                {
                    game.Close();
                    idTaken = true;

                    Console.WriteLine($"id Taken: {id}");
                    return oldGame;
                }
            });

            if (idTaken)
            {
                return null;
            }

            return game;
        }

        public HostedGame FindGame(string id)
        {
            id = id.ToLower();

            HostedGames.TryGetValue(id, out HostedGame result);

            return result;
        }

        public KeyValuePair<string, HostedGame>[] All()
        {
            return HostedGames.ToArray();
        }

        public int Scan(ref NetworkInfo[] info, ScanFilter filter, string passphrase, HostedGame exclude)
        {
            KeyValuePair<string, HostedGame>[] all = HostedGames.ToArray();

            int results = 0;

            for (int i = 0; i < all.Length; i++)
            {
                HostedGame game = all[i].Value;

                if (game.TestReadLock())
                {
                    HostedGames.Remove(game.Id, out HostedGame removed);
                    continue;
                }

                if (game.Passphrase != passphrase || game == exclude)
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
                    if (!scanInfo.NetworkId.SessionId.AsSpan().SequenceEqual(filter.NetworkId.SessionId.AsSpan()))
                    {
                        continue;
                    }
                }

                if (filter.Flag.HasFlag(ScanFilterFlag.Ssid))
                {
                    Span<byte> gameSsid = scanInfo.Common.Ssid.Name.AsSpan()[..scanInfo.Common.Ssid.Length];
                    Span<byte> scanSsid = filter.Ssid.Name.AsSpan()[..filter.Ssid.Length];
                    if (!gameSsid.SequenceEqual(scanSsid))
                    {
                        continue;
                    }
                }

                if (filter.Flag.HasFlag(ScanFilterFlag.NetworkType))
                {
                    if (scanInfo.Common.NetworkType != (byte)filter.NetworkType)
                    {
                        continue;
                    }
                }

                if (game.Players == 0)
                {
                    continue;
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
            HostedGames.Remove(id.ToLower(), out HostedGame removed);
            removed?.Close();
        }

        protected override TcpSession CreateSession()
        {
            return new LdnSession(this);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"LDN TCP server caught an error with code {error}");
        }

        public override bool Stop()
        {
            _cancel.Cancel();

            return base.Stop();
        }

        private async Task BackgroundPingTask()
        {
            while (!IsDisposed)
            {
                foreach (KeyValuePair<Guid, TcpSession> session in Sessions)
                {
                    (session.Value as LdnSession).Ping();
                }

                try
                {
                    await Task.Delay(InactivityPingFrequency, _cancel.Token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }
    }
}
