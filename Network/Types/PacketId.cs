namespace LanPlayServer.Network.Types
{
    enum PacketId
    {
        Passphrase,
        CreateAccessPoint,
        ExternalProxy,
        ExternalProxyToken,
        ExternalProxyState,
        SyncNetwork,
        Scan,
        ScanReply,
        ScanReplyEnd,
        Connect,
        Connected,
        Disconnect,

        ProxyConfig,
        ProxyConnect,
        ProxyConnectReply,
        ProxyData,
        ProxyDisconnect,

        SetAcceptPolicy,
        SetAdvertiseData,

        Ping = 254,
        NetworkError = 255
    }
}
