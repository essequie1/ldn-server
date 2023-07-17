from struct import Struct

from ldn_healthcheck.ryuldn.constants import BYTE_ORDER
from ldn_healthcheck.ryuldn.header import PacketHeader
from ldn_healthcheck.ryuldn.packet import Packet
from ldn_healthcheck.ryuldn.packet_id import PacketId


class CreateAccessPointPacket(Packet):
    __struct = Struct(f"{BYTE_ORDER}HH64s33s15sQHHIHBBH10s16s16siHH")

    def __init__(self):
        super().__init__(PacketId.CreateAccessPoint, self.__struct.size)
        # Security config
        self.security_mode = 0  # ushort
        self.passphrase_size = 0  # ushort
        self.passphrase = b""  # 64 bytes
        # User config
        self.username = b""  # 33 bytes
        self.unknown1 = b""  # 15 bytes
        # Network config
        self.local_communication_id = 0  # ulong (Q)
        self.reserved_1 = 0  # ushort
        self.scene_id = 0  # ushort
        self.reserved_2 = 0  # uint
        self.channel = 0  # ushort
        self.node_count_max = 0  # byte
        self.reserved_3 = 0  # byte
        self.local_communication_version = 0  # ushort
        self.reserved_4 = b""  # 10 bytes
        # RyuNetwork config
        self.game_version = b""  # 16 bytes
        self.private_ip = b""  # 16 bytes
        self.address_family = 0  # int
        self.external_proxy_port = 0  # ushort
        self.internal_proxy_port = 0  # ushort

    def __repr__(self):
        return (
            f"CreateAccessPointPacket(\n"
            f"\tsecurity_mode: {self.security_mode},\n"
            f"\tpassphrase_size: {self.passphrase_size},\n"
            f"\tpassphrase: {self.passphrase},\n"
            f"\tusername: {self.username},\n"
            f"\tunknown1: {self.unknown1},\n"
            f"\tlocal_communication_id: {self.local_communication_id},\n"
            f"\treserved_1: {self.reserved_1},\n"
            f"\tscene_id: {self.scene_id},\n"
            f"\treserved_2: {self.reserved_2},\n"
            f"\tchannel: {self.channel},\n"
            f"\tnode_count_max: {self.node_count_max},\n"
            f"\treserved_3: {self.reserved_3},\n"
            f"\tlocal_communication_version: {self.local_communication_version},\n"
            f"\treserved_4: {self.reserved_4},\n"
            f"\tgame_version: {self.game_version},\n"
            f"\tprivate_ip: {self.private_ip},\n"
            f"\taddress_family: {self.address_family},\n"
            f"\texternal_proxy_port: {self.external_proxy_port},\n"
            f"\tinternal_proxy_port: {self.internal_proxy_port},\n"
            ")"
        )

    def decode(self, buffer: bytes):
        assert len(buffer) >= len(self)
        header = PacketHeader.from_buffer(buffer)
        assert self.__header.packet_id == header.packet_id
        self.__header = header
        (
            self.security_mode,
            self.passphrase_size,
            self.passphrase,
            self.username,
            self.unknown1,
            self.local_communication_id,
            self.reserved_1,
            self.scene_id,
            self.reserved_2,
            self.channel,
            self.node_count_max,
            self.reserved_3,
            self.local_communication_version,
            self.reserved_4,
            self.game_version,
            self.private_ip,
            self.address_family,
            self.external_proxy_port,
            self.internal_proxy_port,
        ) = self.__struct.unpack_from(buffer, len(self.__header))
        self.__header.data_size = self.__struct.size

    def encode(self) -> bytes:
        result = bytearray(len(self))
        result[0 : len(self._Packet__header)] = self._Packet__header.encode()
        self.__struct.pack_into(
            result,
            len(self._Packet__header),
            self.security_mode,
            self.passphrase_size,
            self.passphrase,
            self.username,
            self.unknown1,
            self.local_communication_id,
            self.reserved_1,
            self.scene_id,
            self.reserved_2,
            self.channel,
            self.node_count_max,
            self.reserved_3,
            self.local_communication_version,
            self.reserved_4,
            self.game_version,
            self.private_ip,
            self.address_family,
            self.external_proxy_port,
            self.internal_proxy_port,
        )

        return result
