using System.Runtime.InteropServices;

namespace LanPlayServer.Network.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x19)]
    struct LdnHeader
    {
        public uint Magic;
        public byte Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] UserId;
        public int DataSize;
    }
}