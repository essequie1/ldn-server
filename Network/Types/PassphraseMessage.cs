using LanPlayServer.Utils;
using System.Runtime.InteropServices;

namespace LanPlayServer.Network.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x80)]
    struct PassphraseMessage
    {
        public Array128<byte> Passphrase;
    }
}
