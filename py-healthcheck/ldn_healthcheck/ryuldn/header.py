from struct import Struct

from ldn_healthcheck.ryuldn.constants import BYTE_ORDER, MAGIC, VERSION, MAX_SIZE
from ldn_healthcheck.ryuldn.packet_id import PacketId


class PacketHeader:
    __struct = Struct(f"{BYTE_ORDER}IBB2xi")

    @classmethod
    def from_buffer(cls, buffer: bytes):
        result = cls()
        result.decode(buffer)

        return result

    @classmethod
    def from_default(cls, packet_id: PacketId, data_size: int):
        result = cls()
        result.magic = MAGIC
        result.packet_id = packet_id
        result.version = VERSION
        result.data_size = data_size

        return result

    def __init__(self):
        self.magic = None
        self.packet_id = None
        self.version = None
        self.data_size = None

    def __repr__(self):
        return f"PacketHeader(magic: {self.magic}, id: {self.packet_id}, version: {self.version}, data_size: {self.data_size})"

    def __len__(self):
        return self.__struct.size

    def is_valid(self) -> bool:
        return (
            self.magic == MAGIC
            and self.version == VERSION
            and self.__struct.size + self.data_size < MAX_SIZE
        )

    def decode(self, buffer: bytes):
        assert len(buffer) >= self.__struct.size

        self.magic, packet_id, self.version, self.data_size = self.__struct.unpack(
            buffer[: self.__struct.size]
        )
        self.packet_id = PacketId(packet_id)

    def encode(self) -> bytes:
        return self.__struct.pack(
            self.magic, self.packet_id, self.version, self.data_size
        )
