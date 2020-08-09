using System;
using System.Net;

namespace LanPlayServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 1337;

            Console.WriteLine($"TCP server port: {port}");
            Console.WriteLine();

            var server = new LdnServer(IPAddress.Any, port);

            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine(" done!");

            for (;;)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                    continue;
                }

                line = "(admin) " + line;
                server.Multicast(line);
            }

            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine(" done!");
        }
    }
}