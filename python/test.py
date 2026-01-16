from uavsim import Drone
import cv2

drone = Drone()

drone.takeoff()
# drone.move_forward(120)
# drone.land()

drone.streamon()

frame_reader = drone.get_frame_read()

while True:
    frame = frame_reader.frame
    if frame is not None:
        cv2.imshow("Image", frame)
    cv2.waitKey(1)
