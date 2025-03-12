using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LanPlayServer
{
    internal static class IPBan
    {
        private static ConcurrentBag<string> _bannedIPs;
        private static object _lock = new object();
        private static string _banFilePath = Environment.GetEnvironmentVariable("IP_BAN_FILE_PATH") ?? "bannedips.txt";

        static IPBan()
        {
            _bannedIPs = new ConcurrentBag<string>();
            if (!File.Exists(_banFilePath))
            {
                File.Create(_banFilePath).Close();
            }
            else
            {
                string[] lines = File.ReadAllLines(_banFilePath);
                foreach (string line in lines)
                {
                    _bannedIPs.Add(line);
                }
            }
        }

        public static void BanIP(IPAddress ip)
        {
            try
            {
                string ipString = ip.ToString();
                lock (_lock)
                {
                    if (!_bannedIPs.Contains(ipString))
                    {
                        _bannedIPs.Add(ipString);
                        File.AppendAllText(_banFilePath, ipString + Environment.NewLine);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to ban IP {ip}: {e.Message}");
            }
        }

        public static bool IsIPBanned(IPAddress ip)
        {
            return _bannedIPs.Contains(ip.ToString());
        }

        public static List<string> GetBannedIPs()
        {
            return _bannedIPs.ToList();
        }
    }
}
