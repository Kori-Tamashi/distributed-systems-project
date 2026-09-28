# Лабораторная работа #4

## Deploy to Cloud

### Формулировка

На базе сервисов из предыдущих лабораторных выполнить деплой приложения в managed-кластер Kubernetes: образы docker
публикуются в Docker Registry, манифесты описываются helm chart'ами, наружу сервисы публикуются только через Ingress.
Сборка, публикация образов и деплой выполняются автоматически в Github Actions, после деплоя тем же пайплайном
прогоняется официальная Postman-коллекция (успешный сценарий + отказоустойчивость).

> **База кода.** В `lab4-template` написано «на базе ЛР #2», но официальная Postman-коллекция ЛР4 полностью совпадает с
> коллекцией ЛР3 (папки `success` и `failover/step1-4`; проверено сравнением файлов), т.е. в кластере проверяется
> и отказоустойчивость. Поэтому основой служит код ветки `lab_03` (контракт ЛР2 + Circuit Breaker, fallback и
> retry-очередь ЛР3). Изменения в коде приложения в ЛР4 минимальны — вся работа в инфраструктуре.

> В отличие от шаблона, форк не создаётся: работа ведётся в ветке `lab_04` существующего репозитория
> (`Kori-Tamashi/distributed-systems-project`).

### Требования

1. Развернуть **руками** свой Managed Kubernetes Cluster (достаточно 2–3 worker-нод 2 GB / 1 CPU) и настроить Ingress
   Controller (**ingress-nginx**). Для публикации сервисов наружу используется **только Ingress** (никаких NodePort /
   LoadBalancer-сервисов для самих приложений). Наружу публикуется только Gateway Service — остальные сервисы доступны
   лишь внутри кластера (`ClusterIP`).
2. Собрать и опубликовать образы всех четырёх сервисов в **Docker Hub** (`<DOCKER_USERNAME>/<service>-microservice`).
   Репозитории образов публичные (кластеру не нужен `imagePullSecret`). Публикацию уже выполняет `cd.yml` — для ЛР4 он
   расширяется, а не переписывается.
3. Манифесты для деплоя описать в виде **helm chart'ов**. Chart должен быть **универсальным для всех сервисов** и
   отличаться только набором параметров запуска (`values`): один chart `deploy/helm/microservice` и четыре файла
   значений `deploy/values/<service>.yaml`.
4. В кластере используется **один физический instance PostgreSQL**, но каждый сервис работает только со своей
   виртуальной БД (`flight`, `ticket`, `bonus`; у каждого сервиса своя роль, владеющая только своей БД). PostgreSQL
   разворачивается руками (манифесты в `deploy/k8s/postgres/`).
5. Для каждого сервиса настроены **liveness и readiness probes** на `GET /manage/health` (эндпоинт уже есть на всех
   сервисах с ЛР2).
6. Код хранить на Github, для сборки использовать Github Actions.
7. Для автоматических прогонов заменить `<variant>` на `v1` (в шаблоне — в `autograding.json` и `classroom.yml`; в вашем
   репозитории эту роль выполняют `ci.yml` и `cd.yml`).
8. В пайплайне (`ci.yml` → `cd.yml`) должны быть шаги:
   1. сборка приложения и прогон unit-тестов (уже есть в `ci.yml`);
   2. сборка и публикация docker-образов (уже есть в `cd.yml`);
   3. деплой **каждого** сервиса в кластер k8s (`helm upgrade --install`);
   4. `autograding` — прогон успешного сценария официальной Postman-коллекции против кластера (через Ingress);
   5. **`fault-tolerance` (имя «Fault Tolerance») — отдельный job после `autograding`**, последний в пайплайне:
      остановка/запуск Bonus Service в кластере и прогон `step1`–`step4`.

### Контракт с проверяющим (взят из `lab4-template`, менять нельзя)

| Что | Значение |
|---|---|
| Namespace | `test` |
| Label подов Gateway | `app.kubernetes.io/name=gateway` (шаблон ждёт `kubectl wait -n test -l app.kubernetes.io/name=gateway pod --for=condition=Ready --timeout 3m`) |
| Имена Deployment | `gateway`, `flight`, `ticket`, `bonus` (проверяющий делает `kubectl scale deployment <name> -n test`) |
| Нестабильный сервис в тесте (вариант 1) | **Bonus Service** — Deployment `bonus` |
| Адрес Gateway для Postman | `baseUrl` = публичный адрес Ingress |
| Порты приложений | Gateway `8080`, Flight `8060`, Ticket `8070`, Bonus `8050` |

### Пояснения

1. **Как устроена проверка отказоустойчивости** (логика `scripts/test-script.sh` из `lab4-template`, в пайплайне
   реализуется отдельными шагами job'ов `autograding` и `fault-tolerance`):
   1. папка `success` — обычный happy path;
   2. `kubectl scale deployment bonus -n test --replicas 0` → папка `step1`;
   3. `kubectl scale deployment bonus -n test --replicas 1` (дождаться Ready) → папка `step2`;
   4. `--replicas 0` → папка `step3`;
   5. `--replicas 1` (дождаться Ready) → папка `step4`.

   Сервис останавливают и поднимают **дважды подряд**; остановка выполняется масштабированием Deployment, а не
   `docker stop`, как в ЛР3. Состояние Postman (`ticketUid`, `balance`, …) переносится между запусками через
   `--export-environment`.

2. Коллекция `postman/fault-tolerance-collection.json` (уже лежит в репозитории с ЛР3) идентична коллекции ЛР4 для v1 —
   заменять её не нужно.

3. **Тестовые данные** (должны быть в БД до прогона; в кластере их создаёт шаг деплоя через `psql`):
   ```yaml
   airport:
     - { id: 1, name: Шереметьево, city: Москва,           country: Россия }
     - { id: 2, name: Пулково,     city: Санкт-Петербург, country: Россия }
   flight:
     - { id: 1, flight_number: "AFL031", datetime: "2021-10-08 20:00", from_airport_id: 1, to_airport_id: 2, price: 1500 }
   ```
   плюс запись `privilege` для тестового пользователя — тем же SQL, что и в job'ах `autograding`/`fault-tolerance` ЛР3.

4. Managed Kubernetes предоставляют, например, Digital Ocean, Yandex Cloud, Google Kubernetes Engine, AWS. Провайдер
   ТЗ не фиксирует, от него зависит только способ получения kubeconfig; остальное (helm, kubectl, ingress-nginx)
   одинаково.

### Деградация функциональности и бизнес-правила — Flight Booking System (ваш вариант)

Полностью совпадают с ЛР3 (реализованы в коде приложения, в ЛР4 не меняются).

| Эндпоинт | Поведение при недоступности Bonus Service |
|---|---|
| `GET /api/v1/flights` | Не зависит от Bonus — 200. |
| `GET /api/v1/privilege` | **503**, `{"message": "Bonus Service unavailable"}`. |
| `GET /api/v1/tickets`, `GET /api/v1/tickets/{ticketUid}` | Bonus не используется; при недоступности Flight — fallback в `fromAirport`, `toAirport`, `date`. |
| `GET /api/v1/me` | 200; при недоступности Bonus/Flight — fallback-часть ответа. |
| `POST /api/v1/tickets` | Flight → Ticket → Bonus; при неудаче Bonus откат билета и **503** `Bonus Service unavailable`. |
| `DELETE /api/v1/tickets/{ticketUid}` | Всё равно **204**; откат бонусов ставится в retry-очередь Gateway (повтор каждые 10 с). |

Следствие для k8s: retry-очередь и состояние Circuit Breaker хранятся в памяти пода Gateway, поэтому у Gateway
`replicas: 1`.

### Другие варианты задания (для справки, не относится к вашей работе)

Распределение вариантов такое же, как в ЛР2: **1** Flight Booking System (ваш), **2** Hotels Booking System,
**3** Car Rental System, **4** Library System. Для них отличаются данные, коллекции и сервис, который выключается в
тесте.

### Прием задания

Форк репозитория-шаблона не выполняется. Работа считается выполненной, когда в ветке `lab_04` пайплайн Github Actions
собирает и публикует образы, деплоит все сервисы в кластер, а jobs `autograding` и `fault-tolerance` проходят
успешно.

---

**Источники:**
- https://github.com/bmstu-rsoi/lab4-template
- https://github.com/bmstu-rsoi/lab4-template/blob/master/v1/README.md
