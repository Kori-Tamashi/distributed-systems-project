-- airport + flight для Flight Service
-- Таблицы: airports, flights (с 's')
INSERT INTO airports (id, name, city, country)
VALUES (1, 'Шереметьево', 'Москва', 'Россия'),
       (2, 'Пулково', 'Санкт-Петербург', 'Россия')
ON CONFLICT (id) DO NOTHING;

INSERT INTO flights (id, flight_uid, flight_number, datetime, from_airport_id, to_airport_id, price)
VALUES (1, 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'AFL031', '2021-10-08 20:00', 1, 2, 1500)
ON CONFLICT (id) DO NOTHING;

-- privilege для пользователя Test User (создаётся Bonus Service)
-- Таблица: privilege (без 's'), статус — enum (0=BRONZE, 1=SILVER, 2=GOLD)
INSERT INTO privilege (id, username, status, balance)
VALUES (1, 'Test User', 0, 0)
ON CONFLICT (username) DO NOTHING;
