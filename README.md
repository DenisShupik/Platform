# Platform

`Platform` — форумная платформа с микросервисным бэкендом на ASP.NET Core/.NET 10 и веб-фронтендом на SvelteKit. В репозитории есть локальная инфраструктура через .NET Aspire, фронтенд-приложение, общие библиотеки и генераторы, интеграционные тесты и конфиги для окружения.

## Что уже реализовано

- Форумы, категории, темы и сообщения
- Поток модерации темы: `Draft -> PendingApproval -> Approved` с возвратом в черновик через отклонение
- Аутентификация и роли через Keycloak
- Подписки на темы и внутренние уведомления
- Загрузка аватаров в S3-совместимое хранилище
- Агрегированный OpenAPI в шлюзе и сгенерированный TypeScript-клиент для фронтенда

## Роли

- `User`: может создавать темы, писать сообщения, отправлять тему на модерацию, подписываться на обновления
- `Moderator`: может создавать форумы и категории, подтверждать и отклонять темы
- `Administrator`: верхняя роль в модели авторизации

## Архитектура

### Фронтенд

- `frontend/` — приложение на SvelteKit 2 и Svelte 5
- Авторизация на фронтенде реализована через `better-auth` и Keycloak OAuth
- Фронтенд работает против агрегированного API `ApiGateway`
- Клиентский SDK генерируется из `http://localhost:8000/api/openapi.json`

### Бэкенд-сервисы

- `ApiGateway`: YARP reverse proxy, агрегированный OpenAPI, единая точка входа для фронтенда
- `CoreService`: форумы, категории, темы, сообщения и сценарии модерации
- `UserService`: локальная read-модель пользователей, синхронизируемая по событиям Keycloak через RabbitMQ
- `NotificationService`: подписки, внутренние уведомления, фоновая обработка через TickerQ
- `FileService`: загрузка и удаление аватаров в S3-совместимом хранилище

### Инфраструктура и интеграции

- `PostgreSQL`: основная база данных
- `Valkey`: распределенный кэш и backplane
- `RabbitMQ`: интеграционные события между сервисами
- `Keycloak`: аутентификация, роли и события жизненного цикла пользователя
- `RustFS`: S3-совместимое объектное хранилище для аватаров
- Межсервисное взаимодействие использует и REST, и code-first gRPC (`protobuf-net.Grpc`)

## Структура репозитория

```text
backend/
  ApiGateway/                  Шлюз и агрегированный OpenAPI
  CoreService*/                Домен форумов и модерация
  UserService*/                Пользователи и синхронизация с Keycloak
  NotificationService*/        Подписки и уведомления
  FileService/                 Хранилище аватаров
  DevEnv/                      .NET Aspire AppHost для локальной разработки
  DevEnv.Seeder/               Наполнение демо-данными
  IntegrationTests/            Интеграционные тесты через Aspire.Hosting.Testing
  Shared*/                     Общие абстракции, инфраструктура, presentation и генераторы

frontend/                      Фронтенд на SvelteKit
infrastructure/                Nginx, Keycloak, PostgreSQL и сопутствующие конфиги
```

## Технологический стек

### Бэкенд

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core 10
- Linq2Db
- Wolverine
- protobuf-net.Grpc
- FusionCache
- OpenTelemetry
- .NET Aspire

### Фронтенд

- SvelteKit 2
- Svelte 5
- TypeScript
- Tailwind CSS 4
- better-auth
- bits-ui / shadcn-svelte
- `@hey-api/openapi-ts`

## Что нужно для запуска

- .NET SDK 10
- Docker Desktop
- Node.js
- pnpm

## Локальный запуск

### 1. Запуск бэкенда и инфраструктуры

Запустите Aspire AppHost:

```powershell
dotnet run --project backend/DevEnv/DevEnv.csproj --launch-profile Default
```

Если нужны демо-данные и тестовые пользователи:

```powershell
dotnet run --project backend/DevEnv/DevEnv.csproj --launch-profile WithSeeding
```

Что происходит при старте:

- в контейнерах поднимаются PostgreSQL, Valkey, RabbitMQ, Keycloak и RustFS
- прикладные сервисы стартуют автоматически
- EF Core миграции применяются на старте сервисов
- `ApiGateway` публикует агрегированный OpenAPI и Swagger UI

Полезные адреса:

- Aspire AppHost: `http://localhost:15100`
- Swagger UI: `http://localhost:8000/swagger/index.html`
- Агрегированный OpenAPI: `http://localhost:8000/api/openapi.json`
- Keycloak: `http://localhost:8080`
- RabbitMQ Management UI: `http://localhost:15672`
- RustFS: `http://localhost:9000`
- RustFS Console: `http://localhost:9001`

### 2. Запуск фронтенда

Создайте `frontend/.env` с локальными значениями:

```dotenv
PUBLIC_SSR_API_URL=http://localhost:8000
PUBLIC_CSR_API_URL=http://localhost:8000
PUBLIC_KEYCLOAK_CLIENT_ID=app-user
PUBLIC_AVATAR_URL=http://localhost:9000/avatars
PUBLIC_APP_NAME=PLATFORM

AUTH_KEYCLOAK_ISSUER=http://localhost:8080/realms/app-dev
BETTER_AUTH_URL=http://localhost:5173
BETTER_AUTH_SECRET=replace-with-random-string
```

Затем выполните:

```powershell
cd frontend
pnpm install
pnpm dev
```

Фронтенд будет доступен по адресу `http://localhost:5173`.

### Демо-пользователи для `WithSeeding`

Сидер создает демо-контент и аккаунты. Он стартует после запуска шлюза и содержит небольшую задержку, поэтому после старта окружения стоит немного подождать.

- `moderator` / `12345678`
- `user1` ... `user10` / `12345678`

В демо-набор входят форумы, категории, подтвержденные и отклоненные темы, подписки, уведомления и аватары.

## Порты в разработке

| Компонент | Порт |
| --- | --- |
| Aspire AppHost | `15100` |
| ApiGateway | `8000` |
| CoreService REST | `8010` |
| CoreService gRPC | `8011` |
| UserService REST | `8020` |
| UserService gRPC | `8021` |
| FileService | `8030` |
| NotificationService | `8040` |
| Keycloak | `8080` |
| RustFS | `9000` |
| RustFS Console | `9001` |
| PostgreSQL | `5432` |
| Valkey | `6379` |
| RabbitMQ | `5672` |
| RabbitMQ Management UI | `15672` |

## Команды разработки

### Сборка бэкенда

```powershell
dotnet build backend/Backend.slnx
```

### Запуск интеграционных тестов

```powershell
dotnet test backend/IntegrationTests/IntegrationTests.csproj
```

Примечания:

- тесты используют `Aspire.Hosting.Testing`
- Docker должен быть запущен
- инфраструктура поднимается тестами автоматически

### Обновление API-клиента фронтенда

Сначала поднимите бэкенд, затем выполните:

```powershell
cd frontend
pnpm openapi-ts
```

Клиент генерируется в `frontend/src/lib/utils/client`.

### Сборка фронтенда

```powershell
cd frontend
pnpm build
```

## Важные детали реализации

- `ApiGateway` агрегирует OpenAPI всех бэкенд-сервисов и отдает единую схему для фронтенда
- `UserService` слушает события Keycloak из RabbitMQ и поддерживает собственную read-модель пользователей
- `NotificationService` слушает события `CoreService` и хранит внутренние уведомления и подписки
- `FileService` сейчас работает только с аватарами
- загрузка аватара ограничена `image/webp` и `1 MB`

## Каталог infrastructure

`infrastructure/configs/` содержит артефакты, используемые локальным окружением и деплоем:

- `postgres.sql`: инициализация PostgreSQL
- `keycloak.json`: импортируемый realm-конфиг
- `keycloak-to-rabbit-3.0.5.jar`: мост событий Keycloak в RabbitMQ
