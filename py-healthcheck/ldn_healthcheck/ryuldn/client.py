import socket
import logging
from typing import Optional

from ldn_healthcheck.ryuldn.header import PacketHeader
from ldn_healthcheck.ryuldn.packet import Packet
from ldn_healthcheck.ryuldn.packet_id import PacketId
from ldn_healthcheck.ryuldn.packets.initialize import InitializePacket


class RyujinxLdnClient:
    def __init__(self, host: str, port: int, timeout=30):
        self.socket: socket.socket = None
        self.connect((host, port), timeout)

    def __del__(self):
        if self.socket is not None:
            self.socket.close()
            self.socket = None

    def connect(self, address: tuple[str, int], timeout=30):
        self.socket = socket.create_connection(address, timeout)

    def disconnect(self):
        self.socket.shutdown(socket.SHUT_RDWR)
        self.socket.close()
        self.socket = None

    def send(self, packet: Packet, data: bytes = b""):
        result = packet.encode()
        if len(data) > 0:
            result += data
        logging.info(f"Sending packet with header: {packet._Packet__header}")
        logging.debug(f"Packet: {packet}")
        logging.debug(f"Packet bytearray: {result}")
        self.socket.sendall(result)

    def receive(self) -> tuple[Optional[Packet], bytes]:
        try:
            header_data = self.socket.recv(len(PacketHeader()))
        except TimeoutError:
            logging.exception("Did not receive any data in time.")
            return None, b""

        if len(header_data) == 0:
            logging.error("No data received. Is the socket broken?")
            return None, b""
        header = PacketHeader.from_buffer(header_data)

        chunks = []
        bytes_received = 0
        while bytes_received < header.data_size:
            try:
                chunk = self.socket.recv(min(header.data_size - bytes_received, 2048))
            except TimeoutError:
                logging.exception("Did not receive any data in time.")
                return None, b""

            if len(chunk) == 0:
                logging.error("No chunk data received. Is the socket broken?")
                return None, b""
            bytes_received += len(chunk)
            chunks.append(chunk)

        if len(chunks) == 0:
            logging.error("No chunks received. This should be impossible.")
            return None, b""

        data = header_data
        data += b"".join(chunks)

        match header.packet_id:
            case PacketId.Initialize:
                packet = InitializePacket.from_buffer(data)
                return packet, data[len(packet) :]

            case _:
                return None, data
