from struct import Struct

from ldn_healthcheck.ryuldn.constants import BYTE_ORDER
from ldn_healthcheck.ryuldn.header import PacketHeader
from ldn_healthcheck.ryuldn.packet import Packet
from ldn_healthcheck.ryuldn.packet_id import PacketId


class InitializePacket(Packet):
    __struct = Struct(f"{BYTE_ORDER}16s6s")

    def __init__(self):
        super().__init__(PacketId.Initialize, self.__struct.size)
        self.client_id = b""
        self.mac_address = b""

    def __repr__(self):
        return f"InitializePacket(client_id: {self.client_id}, mac_address: {self.mac_address})"

    def decode(self, buffer: bytearray):
        assert len(buffer) >= len(self)
        header = PacketHeader.from_buffer(buffer)
        assert self._Packet__header.packet_id == header.packet_id
        self.__header = header
        self.client_id, self.mac_address = self.__struct.unpack_from(
            buffer, len(self.__header)
        )
        self.__header.data_size = self.__struct.size

    def encode(self) -> bytearray:
        result = bytearray(len(self))
        result[0 : len(self._Packet__header)] = self._Packet__header.encode()
        self.__struct.pack_into(
            result, len(self._Packet__header), self.client_id, self.mac_address
        )

        return result
