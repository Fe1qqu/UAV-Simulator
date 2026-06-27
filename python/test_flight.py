"""
test_flight.py — тестовый скрипт для проверки всех 7 новых команд.

Запуск:
    python test_flight.py

Убедитесь, что Unity-симулятор запущен и слушает порт 9000.
"""

import time
from uavsim import Drone


def main():
    drone = Drone(host="127.0.0.1", port=9000)

    try:
        # # 1. Взлёт
        # print(">>> takeoff")
        # drone.takeoff()

        # 2. Газ на 80% на 1 секунду (набор высоты)
        print(">>> throttle 0.8 на 1000 мс")
        drone.throttle(power=0.8, duration_ms=100000000)

        # 3. Наклон вперёд на 15° на 800 мс (движение вперёд)
        print(">>> tilt_forward 15° на 800 мс")
        drone.tilt_forward(angle_deg=15, duration_ms=800)

        # 4. Наклон назад на 10° на 500 мс (торможение)
        print(">>> tilt_backward 10° на 500 мс")
        drone.tilt_backward(angle_deg=10, duration_ms=500)

        #  5. Поворот влево на 90°
        print(">>> rotate_left 90°")
        drone.rotate_left(angle_deg=90)

        # 6. Поворот вправо на 45°
        print(">>> rotate_right 45°")
        drone.rotate_right(angle_deg=45)

        # 7. Крен влево на 20° на 600 мс
        print(">>> tilt_left 20° на 600 мс")
        drone.tilt_left(angle_deg=20, duration_ms=600)

        # 8. Крен вправо на 20° на 600 мс
        print(">>> tilt_right 20° на 600 мс")
        drone.tilt_right(angle_deg=20, duration_ms=600)

        # # 9. Посадка
        # print(">>> land")
        # drone.land()

        # print("\nВсе команды выполнены успешно.")

    except Exception as e:
        print(f"[ERROR] {e}")

    finally:
        drone.close()


if __name__ == "__main__":
    main()
