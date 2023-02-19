using System.Runtime.InteropServices;

namespace LanPlayServer.Network.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x2)]
    struct PingMessage
    {
        public byte Requester;
        public byte Id;
    }
}
