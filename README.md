# Task Management System (ASP.NET Core + Vue)

Тестовое приложение для управления задачами с ролевой моделью.

## Быстрый старт

### Предварительные требования
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (опционально)

### Способ 1: Запуск через Docker (рекомендуется)

```bash

# Клонируем репозиторий
git clone https://github.com/blessed234640/TaskManagementSystem2.git
cd TaskManagementSystem2

# Запускаем backend в Docker
docker-compose up -d

# Backend доступен по адресу: http://localhost:8080
# Swagger: http://localhost:8080/swagger

# В отдельном терминале запускаем frontend
cd frontend
npm install
npm run dev
# Frontend: http://localhost:5173
Способ 2: Локальный запуск (для разработки)
Терминал 1 - Backend:

bash
cd TaskManagement.Api
dotnet restore
dotnet run --urls "https://localhost:7073"
# API: https://localhost:7073
# Swagger: https://localhost:7073/swagger
Терминал 2 - Frontend:

bash
cd frontend
npm install
npm run dev
# Frontend: http://localhost:5173
⚠️При первом запуске браузер может ругнуться на SSL сертификат.
Откройте https://localhost:7073 и примите предупреждение.

👥 Ролевая модель
Роль	Права
MANAGER (Начальник)	Создание, редактирование, назначение исполнителя, удаление любых задач
EMPLOYEE (Сотрудник)	Редактирование и изменение статуса только своих задач
VIEWER (Наблюдатель)	Только просмотр задач
🔐 Тестовые учетные записи
После первого запуска база данных автоматически заполняется тестовыми пользователями:

Роль	Email	Пароль
Начальник	manager@example.com	manager123
Сотрудник	employee@example.com	employee123
Наблюдатель	viewer@example.com	viewer123
📚 API Документация
После запуска доступна Swagger документация:

Локально: https://localhost:7073/swagger

В Docker: http://localhost:8080/swagger

Основные эндпоинты:
POST /api/auth/login - получение JWT токена

GET /api/tasks - список задач (с фильтрацией)

POST /api/tasks - создание задачи (только MANAGER)

PUT /api/tasks/{id}/status - изменение статуса

POST /api/tasks/{id}/assign - назначение исполнителя

🏗 Архитектура проекта
text
TaskManagement.Api/
├── Domain/               # Сущности и бизнес-логика
│   ├── Entities/         # User, Task, Role, и т.д.
│   └── Enums/            # TaskStatus, TaskPriority
├── Infrastructure/       # Работа с БД
│   ├── AppDbContext.cs   # EF Core контекст
│   └── DataSeeder.cs     # Начальные данные
├── Auth/                 # JWT аутентификация
├── Services/             # Бизнес-слой
├── Controllers/          # API эндпоинты
└── Transport/            # DTO для запросов/ответов

frontend/                 # Vue 3 приложение
├── src/
│   ├── App.vue          # Основной компонент
│   └── main.js          # Точка входа
└── index.html
🔧 Переменные окружения
Backend (appsettings.json или переменные):
json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tasks.db"  // или для Docker: "/app/data/tasks.db"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-min-16-characters",
    "ExpirationHours": 24
  }
}
Frontend (.env файл):
env
VITE_API_URL=https://localhost:7073  # для локального API
# или
VITE_API_URL=http://localhost:8080   # для API в Docker
❗ Частые проблемы и решения
1. "Не удается подключиться к API"
Проверьте, что backend запущен

Убедитесь, что порты совпадают (7073 или 8080)

Для Docker: docker ps должен показывать запущенный контейнер

2. "Ошибка CORS"
В Development режиме CORS уже настроен

При деплое накатите миграции: dotnet ef database update

3. База данных не создается
bash
cd TaskManagement.Api
dotnet ef database update
📦 Технологии
Backend: .NET 8, EF Core, SQLite, JWT

Frontend: Vue 3, Vite, Fetch API

Контейнеризация: Docker, docker-compose
