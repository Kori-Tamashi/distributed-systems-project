-- ============================================================================
-- Seed data for RSOI Lab 4
-- Safe to run on any database (flight or bonus) - each block checks table existence
-- ============================================================================

-- Airports + flights (only for flight DB where airports table exists)
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.tables
             WHERE table_schema='public' AND table_name='airports') THEN
    INSERT INTO airports (id, name, city, country)
    VALUES (1, 'Шереметьево', 'Москва', 'Россия'),
           (2, 'Пулково', 'Санкт-Петербург', 'Россия')
    ON CONFLICT (id) DO NOTHING;

    INSERT INTO flights (id, flight_uid, flight_number, datetime, from_airport_id, to_airport_id, price)
    VALUES (1, 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'AFL031', '2021-10-08 20:00', 2, 1, 1500)
    ON CONFLICT (id) DO NOTHING;
  END IF;
END $$;

-- Privilege (only for bonus DB where privilege table exists)
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.tables
             WHERE table_schema='public' AND table_name='privilege') THEN
    INSERT INTO privilege (id, username, status, balance)
    VALUES (1, 'testuser', 0, 0)
    ON CONFLICT (username) DO NOTHING;
  END IF;
END $$;
