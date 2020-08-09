using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System.Runtime.InteropServices;

namespace LanPlayServer.Network.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x4FC, CharSet = CharSet.Ansi)]
    struct ConnectRequest
    {
        public ConnectNetworkData Data;
        public NetworkInfo Info;
    }
}
