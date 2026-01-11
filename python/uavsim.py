import socket
import json
import uuid
import logging
from enum import Enum
from typing import Optional, Dict, Any


LOGGER = logging.getLogger("uavsim")
LOGGER.setLevel(logging.INFO)

handler = logging.StreamHandler()
formatter = logging.Formatter("[%(levelname)s] %(message)s")
handler.setFormatter(formatter)

LOGGER.addHandler(handler)


class MessageType(str, Enum):
    COMMAND = "command"
    COMMAND_RESULT = "command_result"
    ERROR = "error"


class Drone:
    server_host: str
    server_port: int
    connection_socket: socket.socket
    
    def __init__(self, host: str = "127.0.0.1", port: int = 9000):
        self.server_host = host
        self.server_port = port

        LOGGER.info("Connecting to UAV simulator at %s:%s", host, port)
        
        self.connection_socket: socket.socket = socket.create_connection((self.server_host, self.server_port))
        self.connection_socket.settimeout(10.0)
        
        LOGGER.info("Connection established")

    # ---------- internal helpers ----------

    def _send_command_and_wait(self, command_name: str, arguments: Optional[Dict[str, Any]] = None) -> None:
        if arguments is None:
            arguments = {}

        command_id = str(uuid.uuid4())

        payload = {
            "message_type": "command",
            "command_name": command_name,
            "command_id": command_id,
            "arguments": arguments
        }

        serialized_payload = json.dumps(payload).encode("utf-8")
        
        # LOGGER.info("Send command: %s", command_name)
        self.connection_socket.sendall(serialized_payload)

        response_raw = self.connection_socket.recv(4096)
        response = json.loads(response_raw.decode("utf-8"))

        if response.get("command_id") != command_id:
            raise RuntimeError("Command/response ID mismatch")

        if response["message_type"] == MessageType.ERROR.value:
            raise RuntimeError(response["error_message"])

        if response["message_type"] != MessageType.COMMAND_RESULT.value:
            raise RuntimeError("Unexpected response type")
        
        LOGGER.info("Response: %s", response.get("status", "ok"))

    # ---------- public API ----------

    def takeoff(self) -> None:
        LOGGER.info("Send command: takeoff")
        self._send_command_and_wait("takeoff")

    # def land(self) -> None:
    #     LOGGER.info("Send command: land")
    #     self._send_command_and_wait("land")

    def move_forward(self, distance_cm: float) -> None:
        LOGGER.info("Send command: move_forward %s cm", distance_cm)
        self._send_command_and_wait(
            "move_forward",
            {"distance_cm": distance_cm}
        )

    # def rotate_clockwise(self, degrees: float) -> None:
    #     LOGGER.info("Send command: rotate_clockwise %s degrees", degrees)
    #     self._send_command_and_wait(
    #         "rotate_clockwise",
    #         {"degrees": degrees}
    #     )

    def close(self) -> None:
        LOGGER.info("Closing connection")
        self.connection_socket.close()
