# РСОИ ЛР4 — Распределённая система на Kubernetes

## Deploy to Cloud (Managed Kubernetes + Helm)

Развёртывание Flight Booking System в managed Kubernetes кластере: 4 микросервиса
(Gateway / Flight / Ticket / Bonus) + единый PostgreSQL instance с тремя виртуальными БД
(`flight`, `ticket`, `bonus`). Образы публикуются в Docker Hub, манифесты описаны
Helm-чартом, наружу публикуется только Gateway через Ingress.

**Seed данных**: `deploy/k8s/postgres/seed.sql` — единый источник истины, безопасен для запуска на любой БД.

### Архитектура

```
                  ┌─────────────────────────────┐
   Postman ──────►│  Ingress (ingress-nginx)    │
                  │  baseUrl = http://<host>/   │
                  └──────────────┬──────────────┘
                                 │
                          ┌──────▼──────┐
                          │  gateway    │  ClusterIP :8080
                          │  (1 replica)│
                          └──┬───┬───┬──┘
                             │   │   │
              ┌──────────────┘   │   └──────────────┐
              │                  │                  │
       ┌──────▼──────┐    ┌──────▼──────┐    ┌──────▼──────┐
       │  flight     │    │  ticket     │    │  bonus      │
       │  :8060      │    │  :8070      │    │  :8050      │
       └──────┬──────┘    └──────┬──────┘    └──────┬──────┘
              │                  │                  │
              └──────────┬───────┴──────────────────┘
                         │
                 ┌───────▼─────────┐
                 │  postgres       │  StatefulSet, ClusterIP :5432
                 │  3 virtual DB   │
                 └─────────────────┘
```

Namespace: `test`
Имена Deployment: `gateway`, `flight`, `ticket`, `bonus`
Порты приложений: Gateway 8080, Flight 8060, Ticket 8070, Bonus 8050

### Публикация наружу (Ingress)

Наружу публикуется **только Gateway** — через ingress-nginx. Остальные сервисы
(flight, ticket, bonus) — ClusterIP и доступны лишь внутри кластера.

- Ingress-контроллер установлен в namespace `ingress-nginx` (helm-релиз `ingress-nginx`).
- В managed-кластере нет cloud-controller-manager → LoadBalancer висит в `<pending>`,
  поэтому фактический адрес — **NodePort ноды**.
- Публичный адрес Gateway: `http://<EXTERNAL-IP-worker-node>:<nodePort>`
  (пример: `http://85.117.235.174:30950`).
- NodePort определяется динамически: `kubectl -n ingress-nginx get svc ingress-nginx-controller -o jsonpath='{.spec.ports[?(@.name=="http")].nodePort}'`.
- Ingress-ресурс создаётся Helm-чартом только для Gateway (`ingress.enabled: true` в `deploy/values/gateway.yaml`).
- В CI/CD (autograding + fault-tolerance) Ingress-адрес резолвится автоматически и передаётся в Postman.

### Структура

```
deploy/
├── helm/microservice/         # универсальный Helm chart (один для всех сервисов)
│   ├── Chart.yaml
│   ├── values.yaml            # defaults
│   └── templates/
│       ├── _helpers.tpl
│       ├── deployment.yaml    # Deployment с liveness/readiness probes на /manage/health
│       ├── service.yaml       # ClusterIP
│       └── ingress.yaml       # условно включается только для gateway
├── values/                    # per-service overrides
│   ├── gateway.yaml           # fullnameOverride=gateway, ingress.enabled=true
│   ├── flight.yaml            # fullnameOverride=flight, DB=flight
│   ├── ticket.yaml            # fullnameOverride=ticket, DB=ticket
│   └── bonus.yaml             # fullnameOverride=bonus,  DB=bonus
└── k8s/postgres/              # PostgreSQL manifests (руками, не через chart)
    ├── namespace.yaml         # namespace=test
    ├── secret.yaml            # POSTGRES_PASSWORD
    ├── configmap-init.yaml    # init.sql: 3 role + 3 database
    ├── statefulset.yaml       # postgres:14-alpine, PVC 1Gi
    └── service.yaml           # ClusterIP :5432
```

### Требования

1. Managed Kubernetes cluster (2–3 worker-ноды, 2GB / 1 CPU) + ingress-nginx.
2. Образы Docker Hub: `<DOCKER_USERNAME>/{gateway,flight,ticket,bonus}-microservice` (публичные).
3. Один физический PostgreSQL instance, три виртуальные БД: `flight`, `ticket`, `bonus`.
4. liveness/readiness probes на `GET /manage/health`.
5. Наружу — только Gateway через Ingress. Остальные сервисы — ClusterIP.
6. CI/CD в GitHub Actions.

### Секреты GitHub

| Secret | Значение |
|---|---|
| `DOCKER_USERNAME` | Docker Hub username |
| `DOCKER_PASSWORD` | Docker Hub access token |
| `KUBE_CONFIG` | kubeconfig (base64 или raw) для managed-кластера |
| `KUBE_NAMESPACE` | `test` |

### Локальная проверка

```bash
# 1. Postgres + сервисы через compose (как в ЛР3)
docker network create rsoi-network
for svc in flight ticket bonus gateway; do
  docker compose -f services/$svc-microservice/docker-compose.$svc-microservice.yml up -d --build
done

# 2. Прогон Postman (success + failover)
newman run postman/collection.json -e postman/environment.json --folder success
```

### Проверка в k8s

```bash
# namespace
kubectl get all -n test

# gateway health через Ingress
curl http://<ingress-host>/manage/health

# прогнать успешный сценарий
newman run postman/collection.json -e postman/environment.json \
  --env-var "baseUrl=http://<ingress-host>" \
  --folder success

# fault-tolerance: остановить Bonus Service
kubectl scale deployment bonus -n test --replicas 0
newman run postman/fault-tolerance-collection.json -e postman/environment.json \
  --env-var "baseUrl=http://<ingress-host>" \
  --folder step1

# поднять обратно
kubectl scale deployment bonus -n test --replicas 1
kubectl wait --for=condition=Ready pod -l app.kubernetes.io/name=bonus -n test --timeout=180s
newman run postman/fault-tolerance-collection.json -e postman/environment.json \
  --env-var "baseUrl=http://<ingress-host>" \
  --folder step2
# ... step3, step4
```

### CI/CD pipeline

`.github/workflows/ci.yml` (по push в `lab_04`):
1. CodeQL security scan
2. Build всех 4 сервисов
3. Unit / integration / API / gateway-api тесты
4. **autograding** — прогон `success` коллекции против docker-контейнеров
5. **fault-tolerance** (ЛР3-режим) — docker stop/start bonus-api-e2e + step1–step4

`.github/workflows/cd.yml` (после успешного CI):
1. **docker-build-and-push** — сборка и публикация 4 образов в Docker Hub
2. **deploy-k8s** — применение Postgres manifests + `helm upgrade --install` для 4 сервисов
   + seed тестовых данных в БД `flight` и `bonus`
3. **autograding** — прогон `success` коллекции против Ingress
4. **fault-tolerance** (ЛР4-режим) — `kubectl scale deployment bonus -n test --replicas 0/1` + step1–step4

### Тестовые данные

Сидятся в `cd.yml` job `deploy-k8s` сразу после готовности PostgreSQL:

- `airports`: Шереметьево (Москва), Пулково (Санкт-Петербург)
- `flights`: AFL031, 2021-10-08 20:00, price=1500
- `privilege`: Test User (BRONZE, balance=0)

### Деградация функциональности

| Эндпоинт | Bonus недоступен | Flight недоступен |
|---|---|---|
| `GET /api/v1/flights` | 200 | **500** (критичный) |
| `GET /api/v1/privilege` | **503** `Bonus Service unavailable` | 200 |
| `GET /api/v1/tickets`, `/tickets/{uid}` | 200 | 200, fallback для fromAirport/toAirport/date |
| `GET /api/v1/me` | 200, fallback privilege=null | 200, fallback |
| `POST /api/v1/tickets` | откат Ticket + **503** | **500** |
| `DELETE /api/v1/tickets/{uid}` | **204**, откат бонусов в retry-очередь | 204 |

Circuit Breaker + in-memory retry queue хранятся в pod Gateway → `replicas: 1` обязателен.

### Контракт с проверяющим

| Что | Значение |
|---|---|
| Namespace | `test` |
| Label подов Gateway | `app.kubernetes.io/name=gateway` |
| Имена Deployment | `gateway`, `flight`, `ticket`, `bonus` |
| Нестабильный сервис (v1) | `bonus` |
| baseUrl для Postman | публичный адрес Ingress |
| Порты | 8080 / 8060 / 8070 / 8050 |
