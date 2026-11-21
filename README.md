# CrossApp — Система обліку бібліотечних видач

Кросплатформний консольний застосунок на **.NET 10** для управління бібліотечним фондом та видачами книг (`Loan`), що підтримує типізовану ієрархічну конфігурацію, профілі середовищ, безпечне зберігання секретів та атомарну файлову персистентність.

## Структура проєкту

- `src/Core` — доменні моделі (`Book`, `BookCopy`, `Reader`, `Loan`), DTO, типізована конфігурація (`AppOptions`), сервіси (`LendingService`) та файлове сховище (`FileLoanStore`).
- `src/Cli` — консольний застосунок, збирання конфігурації (`ConfigurationBuilder`), команди `import` та `export`.
- `tests/Core.Tests` — модульні тести (`xUnit`) для перевірки доменних інваріантів, серіалізації та сховища.

---

## Профілі середовища (`Development` та `Production`)

Активний профіль задається змінною середовища `CROSSAPP_ENVIRONMENT` (якщо змінну не встановлено, за замовчуванням використовується `Development`).

- **`Development`** (`appsettings.Development.json`) — використовується під час локальної розробки та тестування. Має детальний рівень логування (`Debug`), зберігає дані в окремий файл `library-dev.json` і встановлює скорочений термін видачі (`7` днів).
- **`Production`** (`appsettings.json`) — базова конфігурація для робочого середовища. Фіксує лише попередження та помилки (`Warning`), використовує основний файл `library.json` і стандартний термін видачі (`14` днів).

Базовий каталог для збереження даних будується відносно каталогу скомпільованого застосунку (`AppContext.BaseDirectory`), що гарантує однаковий шлях незалежно від поточного робочого каталогу (`cwd`).

---

## Таблиця ключів конфігурації

| Ключ конфігурації | Змінна середовища | За замовчуванням (`Production`) | Профіль `Development` | Опис |
| :--- | :--- | :--- | :--- | :--- |
| `EnvironmentName` | `CROSSAPP_ENVIRONMENT` | `Production` | `Development` | Назва активного профілю середовища |
| `Storage:DataPath` | `CROSSAPP_Storage__DataPath` або `CROSSAPP_DATA_PATH` | `data` | `data` | Шлях до каталогу збереження JSON-файлів (відносний або абсолютний) |
| `Storage:FileName` | `CROSSAPP_Storage__FileName` | `library.json` | `library-dev.json` | Ім'я файлу сховища видач |
| `Library:LoanDays` | `CROSSAPP_Library__LoanDays` | `14` | `7` | Стандартний термін видачі книги у днях |
| `Logging:MinimumLevel` | `CROSSAPP_Logging__MinimumLevel` | `Warning` | `Debug` | Мінімальний рівень логування застосунку |
| `Integration:ApiKey` | `CROSSAPP_Integration__ApiKey` *(або `user-secrets`)* | *(не задано)* | *(через `user-secrets`)* | Конфіденційний ключ інтеграції (не зберігається у Git) |

---

## Налаштування секретів (`user-secrets`)

Для безпечного локального зберігання `Integration:ApiKey` поза межами репозиторію використовується менеджер секретів .NET:

```bash
dotnet user-secrets init --project src/Cli
dotnet user-secrets set "Integration:ApiKey" "your-secret-key" --project src/Cli
