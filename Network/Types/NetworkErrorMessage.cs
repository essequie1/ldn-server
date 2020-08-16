using System.Runtime.InteropServices;

namespace LanPlayServer.Network.Types
{
    enum NetworkError : int
    {
        None,

        PortUnreachable,

        TooManyPlayers,
        VersionTooLow,
        VersionTooHigh,

        Unknown = -1
    }

    [StructLayout(LayoutKind.Sequential, Size = 0x4)]
    struct NetworkErrorMessage
    {
        public NetworkError Error;
    }
}
