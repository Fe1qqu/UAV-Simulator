import socket
import threading
import cv2
import numpy as np
import logging
from collections import defaultdict

LOGGER = logging.getLogger("uavsim.video")


class VideoFrameReader:
    def __init__(self, udp_port: int = 11111):
        self.udp_port = udp_port

        self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.socket.bind(("0.0.0.0", udp_port))

        self.frame = None
        self.stopped = False

        self._frames_buffer = defaultdict(dict)
        
        self._thread = threading.Thread(target=self._worker, daemon=True)
        self._thread.start()

        LOGGER.info("Video stream reader started on UDP port %s", udp_port)

    def _worker(self):
        while not self.stopped:
            try:
                data, _ = self.socket.recvfrom(65536)
                if len(data) < 6:
                    continue

                frame_id = (data[0] << 8) | data[1]
                packet_idx = (data[2] << 8) | data[3]
                total_packets = (data[4] << 8) | data[5]

                payload = data[6:]

                self._frames_buffer[frame_id][packet_idx] = payload

                if len(self._frames_buffer[frame_id]) == total_packets:
                    full_data = b''.join(self._frames_buffer[frame_id][i] for i in range(total_packets))
                    image = cv2.imdecode(np.frombuffer(full_data, np.uint8), cv2.IMREAD_COLOR)
                    if image is not None:
                        self.frame = image

                    del self._frames_buffer[frame_id]

            except Exception as exc:
                LOGGER.warning("Video stream error: %s", exc)

    def stop(self):
        self.stopped = True
        try:
            self.socket.close()
        except OSError:
            pass
