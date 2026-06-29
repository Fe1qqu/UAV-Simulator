import time
from uavsim import Drone


def main():
    drone = Drone(host="127.0.0.1", port=9000)

    try:
        # 1. Газ на 80% на 5 секунд (набор высоты)
        drone.throttle(power=0.8, duration_ms=5000)

        # 2. Наклон вперёд на 15° на 2000 мс (движение вперёд)
        drone.tilt_forward(angle_deg=15, duration_ms=2000)

        # 3. Наклон назад на 10° на 500 мс (торможение)
        drone.tilt_backward(angle_deg=10, duration_ms=500)

        # 4. Поворот влево на 180°
        drone.rotate_left(angle_deg=180)

        # 5. Поворот вправо на 90°
        drone.rotate_right(angle_deg=90)

        # 6. Крен влево на 20° на 1000 мс
        drone.tilt_left(angle_deg=20, duration_ms=1000)

        # 7. Крен вправо на 20° на 1000 мс
        drone.tilt_right(angle_deg=20, duration_ms=1000)

    except Exception as e:
        print(f"[ERROR] {e}")

    finally:
        drone.close()


if __name__ == "__main__":
    main()
