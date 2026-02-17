## Task Management (ASP.NET Core + Vue)

Полноценное тестовое приложение для управления задачами с ролевой моделью:

- **Начальник (MANAGER)** — создаёт, редактирует, назначает и завершает задачи, может удалять.
- **Сотрудник (EMPLOYEE)** — работает только со своими задачами (менять статус/редактировать/завершать).
- **Наблюдатель (VIEWER)** — только просматривает задачи.

Приложение состоит из:

- backend‑API на **ASP.NET Core 8 + EF Core + SQLite**;
- минимального frontend на **Vue 3 (Vite)** для демонстрации авторизации и прав.

---

### Архитектура backend

- `TaskManagement.Api` — ASP.NET Core Web API (`net8.0`).
- **Слои и папки**:
  - `Domain/Entities` — доменные сущности (`User`, `Department`, `Role`, `Permission`, `TaskItem`).
  - `Domain/Enums` — перечисления `TaskStatus` (`New/InProgress/Done`) и `TaskPriority` (`Low/Medium/High`).
  - `Infrastructure/AppDbContext` — EF Core контекст, связи и индексы.
  - `Infrastructure/DataSeeder` — сидинг ролей, пользователей, отделов и примерной задачи.
  - `Infrastructure/PasswordHasher` — упрощённое SHA256‑хэширование паролей (для теста).
  - `Auth/JwtSettings` и `Auth/JwtTokenService` — настройка и генерация JWT.
  - `Services/TaskService` + `Services/Interfaces/ITaskService` — бизнес‑логика задач с учётом ролей.
  - `Transport/*Dtos` — DTO для задач, фильтров и логина.
  - `Controllers/*Controller` — REST‑эндпоинты (`Auth`, `Tasks`, `Users`, `Departments`).

- **База данных**:
  - SQLite файл по умолчанию: `tasks.db` (локальный запуск).
  - В Docker путь переопределяется через `ConnectionStrings__DefaultConnection=Data Source=/app/data/tasks.db`.
  - Миграции EF Core (создаются командой `dotnet ef migrations add InitialCreate`).

- **Аутентификация и авторизация**:
  - JWT Bearer (заголовок `Authorization: Bearer <token>`).
  - В payload токена: `sub` (Id пользователя), `name`, `email`, `role` (MANAGER/EMPLOYEE/VIEWER).
  - Все контроллеры, кроме `AuthController.Login`, закрыты `[Authorize]`.

- **Swagger**:
  - В `Development` среде доступен по `/swagger`.

---

### Роли и права

- **MANAGER (Начальник)**:
  - создавать задачи;
  - редактировать любые задачи;
  - менять статус и приоритет;
  - назначать исполнителей;
  - завершать и удалять задачи.

- **EMPLOYEE (Сотрудник)**:
  - редактировать только назначенные ему задачи;
  - менять статус только у своих задач;
  - завершать только свои задачи.

- **VIEWER (Наблюдатель)**:
  - только просмотр списка задач (никаких изменений).

---

### Тестовые учётные записи

Seed‑данные создаются автоматически при первом запуске backend:

- **Начальник**  
  - Email: `manager@example.com`  
  - Пароль: `manager123`  
  - Роль: `MANAGER`

- **Сотрудник**  
  - Email: `employee@example.com`  
  - Пароль: `employee123`  
  - Роль: `EMPLOYEE`

- **Наблюдатель**  
  - Email: `viewer@example.com`  
  - Пароль: `viewer123`  
  - Роль: `VIEWER`

---

### Frontend (Vue 3 + Vite)

Минимальный SPA в папке `frontend`:

- форма логина (email/пароль) → `POST /api/auth/login`;
-.сохранение JWT в `localStorage` и отображение имени/роли;
- список задач (`GET /api/tasks`), подгружаются с учётом CORS;
- для роли **MANAGER**:
  - форма создания задач (`title`, `description`, `priority`, `assignedToUserId`);
  - назначение исполнителя (`POST /api/tasks/{id}/assign`);
  - смена статуса (`POST /api/tasks/{id}/status`);
- статусы и приоритеты отображаются человекочитаемо:
  - Статусы: `0` → «Новая», `1` → «В работе», `2` → «Завершена»;
  - Приоритеты: `0` → «Низкий», `1` → «Средний», `2` → «Высокий».

По умолчанию фронт в dev‑режиме обращается к API по `https://localhost:7073`.

---

## Локальный запуск **без Docker**

### 1. Запуск backend

В одном терминале:

```bash
cd "C:\Users\Victus\OneDrive\Рабочий стол\DotNet"

# (опционально) сборка
dotnet build TaskManagement.Api/TaskManagement.Api.csproj

# запуск только на HTTPS-порту
dotnet run --project TaskManagement.Api/TaskManagement.Api.csproj --urls "https://localhost:7073"
```

Backend:

- Swagger: `https://localhost:7073/swagger`
- API: `https://localhost:7073`

> При первом запуске EF Core применит миграции, создаст `tasks.db` и seed‑данные (роли/права/пользователи/пример задачи).

### 2. Запуск frontend

Во втором терминале:

```bash
cd "C:\Users\Victus\OneDrive\Рабочий стол\DotNet\frontend"
npm install   # один раз
npm run dev
```

Фронт:

- Vite dev server: `http://localhost:5173`
- API: `https://localhost:7073`

Если браузер ругается на HTTPS‑сертификат:

1. Открой `https://localhost:7073` в отдельной вкладке.
2. Согласись с предупреждением (продолжить к сайту).
3. Обнови `http://localhost:5173`.

---

## Запуск backend в Docker

Для удобства backend упакован в Docker; база (`tasks.db`) хранится в volume `tasks-data`.

### Вариант 1: `docker compose` (рекомендуется)

В корне репозитория (`TaskManagement.sln`):

```bash
docker compose build
docker compose up -d
```

Что произойдёт:

- соберётся образ `taskmanagement-api` на основе `TaskManagement.Api/Dockerfile`;
- запустится контейнер `taskmanagement-api`:
  - внутренняя ссылка: `http://0.0.0.0:8080`;
  - внешний порт: `http://localhost:8080`;
  - подключён volume `tasks-data:/app/data` для файла `tasks.db`.

Доступ:

- Swagger: `http://localhost:8080/swagger`
- API: `http://localhost:8080`

Остановка:

```bash
docker compose down
```

### Вариант 2: чистый `docker build` + `docker run`

Если не нужен `docker-compose.yml`:

```bash
cd "C:\Users\Victus\OneDrive\Рабочий стол\DotNet"

docker build -t taskmanagement-api -f TaskManagement.Api/Dockerfile .

docker run -d ^
  -p 8080:8080 ^
  -e ConnectionStrings__DefaultConnection="Data Source=/app/data/tasks.db" ^
  -e ASPNETCORE_ENVIRONMENT=Development ^
  -v tasks-data:/app/data ^
  --name taskmanagement-api ^
  taskmanagement-api
```

Те же адреса:

- Swagger: `http://localhost:8080/swagger`
- API: `http://localhost:8080`

Остановить и удалить контейнер:

```bash
docker stop taskmanagement-api
docker rm taskmanagement-api
```

> Обрати внимание: фронтенд пока не контейнеризирован и запускается через `npm run dev`. Для Docker‑варианта фронта можно отдельно добавить контейнер с Vite или собранным статическим SPA, но для тестового задания достаточно текущего подхода.

---

## Запуск frontend при backend в Docker

Если backend запущен через Docker и слушает `http://localhost:8080`, то есть два варианта:

- **Вариант 1** (простой, текущий): оставить API на `https://localhost:7073` и запускать backend обычным `dotnet run` — это уже работает «из коробки».
- **Вариант 2**: при желании поменять `apiBaseUrl` в `frontend/src/App.vue` на `http://localhost:8080` и перезапустить `npm run dev`, чтобы фронт ходил в контейнеризированный backend.

Сейчас в коде по умолчанию используется `https://localhost:7073`, так как основной упор делался на демонстрацию логики backend и ролей без Docker.

---

## Git и .gitignore

В корне лежит файл `.gitignore`, настроенный для:

- артефактов .NET (`bin/`, `obj/`);
- IDE (`.vs/`, `.idea/`, `.vscode/`);
- локальной SQLite‑базы (`tasks.db`, `*.db-shm`, `*.db-wal`);
- frontend‑артефактов (`frontend/node_modules/`, `frontend/dist/`, `frontend/.vite/`, локальные `.env`‑файлы).

Это позволяет безопасно выполнить:

```bash
git init
git add .
git commit -m "Initial task management implementation"
```

и запушить репозиторий, не подхватив лишние временные файлы, локальную базу или node_modules.

