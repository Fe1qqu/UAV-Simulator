import socket
import json
import uuid
import logging
from enum import Enum
from typing import Optional, Dict, Any

from video_frame_reader import VideoFrameReader

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
    _video_reader: Optional[VideoFrameReader]

    def __init__(self, host: str = "127.0.0.1", port: int = 9000):
        self.server_host = host
        self.server_port = port

        LOGGER.info("Connecting to UAV simulator at %s:%s", host, port)

        self.connection_socket: socket.socket = socket.create_connection(
            (self.server_host, self.server_port)
        )
        self.connection_socket.settimeout(60.0)

        LOGGER.info("Connection established")

        self._video_reader: Optional[VideoFrameReader] = None

    # ---------- internal helpers ----------

    def _send_command_and_wait(
        self,
        command_name: str,
        arguments: Optional[Dict[str, Any]] = None,
    ) -> None:
        if arguments is None:
            arguments = {}

        command_id = str(uuid.uuid4())

        payload = {
            "message_type": "command",
            "command_name": command_name,
            "command_id": command_id,
            "arguments": arguments,
        }

        serialized_payload = json.dumps(payload).encode("utf-8")
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

    def land(self) -> None:
        LOGGER.info("Send command: land")
        self._send_command_and_wait("land")

    def move_forward(self, distance_cm: float) -> None:
        LOGGER.info("Send command: move_forward %s cm", distance_cm)
        self._send_command_and_wait("move_forward", {"distance_cm": distance_cm})

    def throttle(self, power: float, duration_ms: float) -> None:
        """Включить газ на power*100% на duration_ms мс.

        Args:
            power: мощность от 0.0 до 1.0 (0.5 ≈ hover).
            duration_ms: длительность в миллисекундах.
        """
        LOGGER.info("Send command: throttle power=%.2f duration_ms=%s", power, duration_ms)
        self._send_command_and_wait(
            "throttle",
            {"power": float(power), "duration_ms": float(duration_ms)},
        )

    def tilt_forward(self, angle_deg: float, duration_ms: float) -> None:
        """Наклон вперёд на angle_deg градусов на duration_ms мс.

        Args:
            angle_deg: угол наклона вперёд (0..maxTiltAngle, обычно 30°).
            duration_ms: длительность удержания наклона в миллисекундах.
        """
        LOGGER.info("Send command: tilt_forward angle=%s° duration_ms=%s", angle_deg, duration_ms)
        self._send_command_and_wait(
            "tilt_forward",
            {"angle_deg": float(angle_deg), "duration_ms": float(duration_ms)},
        )

    def tilt_backward(self, angle_deg: float, duration_ms: float) -> None:
        """Наклон назад на angle_deg градусов на duration_ms мс."""
        LOGGER.info("Send command: tilt_backward angle=%s° duration_ms=%s", angle_deg, duration_ms)
        self._send_command_and_wait(
            "tilt_backward",
            {"angle_deg": float(angle_deg), "duration_ms": float(duration_ms)},
        )

    def rotate_left(self, angle_deg: float) -> None:
        """Поворот влево (yaw) на angle_deg градусов. Ждёт завершения поворота.

        Args:
            angle_deg: угол поворота в градусах (всегда положительный).
        """
        LOGGER.info("Send command: rotate_left angle=%s°", angle_deg)
        self._send_command_and_wait(
            "rotate_left",
            {"angle_deg": float(angle_deg)},
        )

    def rotate_right(self, angle_deg: float) -> None:
        """Поворот вправо (yaw) на angle_deg градусов. Ждёт завершения поворота."""
        LOGGER.info("Send command: rotate_right angle=%s°", angle_deg)
        self._send_command_and_wait(
            "rotate_right",
            {"angle_deg": float(angle_deg)},
        )

    def tilt_left(self, angle_deg: float, duration_ms: float) -> None:
        """Крен влево на angle_deg градусов на duration_ms мс."""
        LOGGER.info("Send command: tilt_left angle=%s° duration_ms=%s", angle_deg, duration_ms)
        self._send_command_and_wait(
            "tilt_left",
            {"angle_deg": float(angle_deg), "duration_ms": float(duration_ms)},
        )

    def tilt_right(self, angle_deg: float, duration_ms: float) -> None:
        """Крен вправо на angle_deg градусов на duration_ms мс."""
        LOGGER.info("Send command: tilt_right angle=%s° duration_ms=%s", angle_deg, duration_ms)
        self._send_command_and_wait(
            "tilt_right",
            {"angle_deg": float(angle_deg), "duration_ms": float(duration_ms)},
        )

    def streamon(self) -> None:
        LOGGER.info("Send command: streamon")
        self._send_command_and_wait("streamon")

        if self._video_reader is None:
            self._video_reader = VideoFrameReader()

    def streamoff(self) -> None:
        LOGGER.info("Send command: streamoff")
        self._send_command_and_wait("streamoff")

        if self._video_reader is not None:
            self._video_reader.stop()
            self._video_reader = None

    def get_frame_read(self):
        if self._video_reader is None:
            raise RuntimeError("Video stream is not enabled. Call streamon() first.")
        return self._video_reader

    def close(self) -> None:
        LOGGER.info("Closing connection")
        self.connection_socket.close()
