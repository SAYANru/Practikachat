# QuickChat Server

Серверная часть мессенджера **QuickChat**, реализованная на платформе ASP.NET Core с поддержкой обмена сообщениями в реальном времени через SignalR и JWT-аутентификацией.

## 📌 Особенности

- Реализация REST API для управления пользователями, чатами и сообщениями
- Обмен сообщениями в реальном времени через SignalR
- JWT-аутентификация
- PostgreSQL как основная СУБД
- Поддержка масштабируемости (Redis, PgBouncer)
- CI/CD, мониторинг, безопасность

## 🧩 Технологический стек

### Бэкенд

| Технология            | Назначение                             |
|------------------------|----------------------------------------|
| ASP.NET Core 6.0       | REST API, SignalR                      |
| Entity Framework Core  | ORM для PostgreSQL                     |
| SignalR                | WebSocket-соединения                   |
| JWT Bearer Auth        | Аутентификация                         |
| FluentValidation       | Валидация DTO                          |
| AutoMapper             | Преобразование моделей <-> DTO         |

### База данных

| Технология    | Назначение                                 |
|---------------|--------------------------------------------|
| PostgreSQL 14+| Основная реляционная БД                    |
| Redis 6.2     | Кэширование и pub/sub                      |
| PgBouncer     | Пул соединений                             |

### Инфраструктура

- Docker
- Nginx
- GitHub Actions
- Prometheus + Grafana

### Безопасность

- Let's Encrypt (SSL)
- Rate Limiting (ASP.NET Middleware)
- BCrypt.Net (хеширование паролей)

## 📒 Документация API

### Аутентификация

| Метод | Путь               | Описание                |
|-------|--------------------|-------------------------|
| POST  | /api/auth/register | Регистрация пользователя|
| POST  | /api/auth/login    | Вход в систему          |

### Чаты

| Метод | Путь         | Описание             |
|-------|--------------|----------------------|
| GET   | /api/chats   | Получить список чатов|
| POST  | /api/chats   | Создать новый чат    |

### Сообщения

| Метод | Путь           | Описание                        |
|-------|----------------|---------------------------------|
| GET   | /api/messages  | Получить сообщения из чата     |
| POST  | /api/messages  | Отправить сообщение             |

## 🔒 Аутентификация

- После логина выдается JWT-токен.
- Токен используется для всех последующих запросов.
- Проверка производится middleware’ом и внутри SignalR хаба.

## ⚙️ Установка и запуск

### Требования

- Windows Server 2019+ / Linux (Ubuntu 20.04+)
- .NET 6.0 SDK
- PostgreSQL 14+

### Подготовка базы данных

```sql
CREATE DATABASE quickchat_db;
CREATE USER quickchat_user WITH PASSWORD 'Ваш_пароль';
GRANT ALL PRIVILEGES ON DATABASE quickchat_db TO quickchat_user;
```

В `appsettings.json`:

```json
"ConnectionStrings": {
  "PostgreSQL": "Host=localhost;Port=5432;Database=quickchat_db;Username=quickchat_user;Password=Ваш_пароль;Pooling=true;"
}
```

### Запуск проекта

```bash
git clone https://github.com/SAYANru/Practikachat.git
cd Practikachat
dotnet restore
dotnet ef database update --project QuickChat.Server
dotnet run --project QuickChat.Server
```

## ✅ Тестирование

- `xUnit` — модульные тесты
- `Moq` — мокирование зависимостей
- `Postman` — интеграционные тесты API

## 📎 Дополнительно

- Swagger UI доступен по пути `/swagger`
- Поддержка масштабирования через Redis Pub/Sub
- Диаграммы классов и последовательностей: PlantUML

## 📂 Репозиторий

(https://github.com/SAYANru/Practikachat)
