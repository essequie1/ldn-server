using System.Runtime.InteropServices;

namespace LanPlayServer.Network.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x1, Pack = 1)]
    struct SetAcceptPolicyRequest
    {
        public byte StationAcceptPolicy;
    }
}
