# Student API

Цей проєкт містить простий REST API для роботи зі списком студентів. API реалізовано на ASP.NET Core Minimal API з використанням Dapper та PostgreSQL.

## Запуск

1. Встановіть .NET 7 SDK.
2. Перейдіть у каталог `StudentApi`.
3. Виконайте команди:
   ```bash
   dotnet restore
   dotnet run
   ```
4. За замовчуванням застосунок стартує на `https://localhost:5001` та `http://localhost:5000`.
5. Відкрийте у браузері `https://localhost:5001` (або `http://localhost:5000`), щоб скористатися вбудованою XHTML-сторінкою для перевірки API без Postman.

> **Примітка щодо БД.** За замовчуванням використовується підключення до PostgreSQL `romashkadb_shp8` (див. `appsettings.json`). Ви можете перевизначити рядок підключення через змінну оточення `ConnectionStrings__Default`. URI у форматі `postgresql://user:pass@host:port/database` автоматично перетворюється на формат, який розуміє Npgsql.

> **Примітка.** Якщо під час збірки з'являється помилка `project.assets.json not found`, переконайтеся, що попередньо виконано `dotnet restore`. Ця команда створює файл ресурсів з залежностями NuGet.

## Маршрути

- `GET /api/students` — повертає список студентів. Доступні параметри фільтрації:
  - `group` — точна назва групи.
  - `minGpa` — мінімальне значення середнього балу.
- `GET /api/students/{id}` — повертає студента за ідентифікатором.
- `POST /api/students` — створює нового студента.
- `PUT /api/students/{id}` — оновлює дані студента.
- `DELETE /api/students/{id}` — видаляє студента.

Усі відповіді повертаються у форматі JSON.
