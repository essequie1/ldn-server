from enum import IntEnum, auto


class PacketId(IntEnum):
    Initialize = 0
    Passphrase = auto()

    CreateAccessPoint = auto()
    CreateAccessPointPrivate = auto()
    ExternalProxy = auto()
    ExternalProxyToken = auto()
    ExternalProxyState = auto()
    SyncNetwork = auto()
    Reject = auto()
    RejectReply = auto()
    Scan = auto()
    ScanReply = auto()
    ScanReplyEnd = auto()
    Connect = auto()
    ConnectPrivate = auto()
    Connected = auto()
    Disconnect = auto()

    ProxyConfig = auto()
    ProxyConnect = auto()
    ProxyConnectReply = auto()
    ProxyData = auto()
    ProxyDisconnect = auto()

    SetAcceptPolicy = auto()
    SetAdvertiseData = auto()

    Ping = 254
    NetworkError = 255
