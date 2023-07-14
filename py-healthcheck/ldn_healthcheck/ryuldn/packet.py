from abc import ABCMeta, abstractmethod
from struct import Struct

from ldn_healthcheck.ryuldn.header import PacketHeader
from ldn_healthcheck.ryuldn.packet_id import PacketId


class Packet(metaclass=ABCMeta):
    __struct: Struct

    @classmethod
    def from_buffer(cls, buffer: bytes):
        # noinspection PyArgumentList
        # This is only supposed to be used by subclasses
        result = cls()
        result.decode(buffer)
        return result

    def __init__(self, packet_id: PacketId, data_size: int):
        self.__header = PacketHeader.from_default(packet_id, data_size)

    def __len__(self):
        return len(self.__header) + self.__header.data_size

    def get_size(self) -> int:
        return self.__struct.size

    def get_id(self):
        return self.__header.packet_id

    @abstractmethod
    def decode(self, buffer: bytes):
        pass

    @abstractmethod
    def encode(self) -> bytes:
        pass
