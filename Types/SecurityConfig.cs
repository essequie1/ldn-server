using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x44, CharSet = CharSet.Ansi)]
    struct SecurityConfig
    {
        public ushort SecurityMode;
        public ushort PassphraseSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)]
        public byte[] Passphrase;
    }
}