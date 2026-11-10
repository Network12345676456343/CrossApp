# CrossApp — Лабораторна робота 8

async/await, скасування, доменні винятки, unit-тести Core

---

## 1. Запуск unit-тестів

Усі модульні тести розміщені в ізольованому проєкті `tests/Core.Tests` (xUnit), який посилається виключно на збірку `Core`:

```bash
dotnet test
```

### Результат виконання тестів:

- **Усього тестів:** 10
- **Успішних:** 10
- **Помилок:** 0
- **Пропущених:** 0

Набір тестів покриває:

- Інваріанти сутностей (`BookCopy.Issue`, `BookCopy.Return`).
- Одноразову генерацію події `LendingService.Changed`.
- Детерміновані розрахунки звітів у `ReportServiceTests` (`[Theory]`, `[InlineData]`).
- Миттєву реакцію на попередньо скасований токен (`OperationCanceledException`).
- Відхилення пошкоджених даних із тимчасового файлу з гарантованим очищенням у блоці `finally`.
- Спрацювання таймауту у зв'язаних токенах (`CreateLinkedTokenSource`).

## 2. Збір тестового покриття (Code Coverage)

Для генерації звіту про покриття коду тестами через вбудований збирач Coverlet:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Звіт у форматі `coverage.cobertura.xml` зберігається за шляхом:

`tests/Core.Tests/TestResults/<GUID>/coverage.cobertura.xml`.

## 3. Запуск CLI та сценарії поведінки

Імпорт каталогу викликається передачею прапорця `--file`:

```bash
dotnet run --project src/Cli -- --file data/sample.json
```

### Сценарії перевірки:

**1. Успішний імпорт:**

```bash
dotnet run --project src/Cli -- --file data/sample.json
```

**Вивід:**

```text
[Прогрес] Опрацьовано записів: 17...
Імпортовано записів: 17
```

**2. Перевірка скасування (Ctrl+C або таймаут):**

- Під час імпорту натиснути `Ctrl+C` або вичерпати ліміт таймауту (`timeoutCts`).
- Застосунок штатно перехоплює `OperationCanceledException` без аварійного стек-трейсу.

**Вивід:**

```text
Імпорт скасовано.
```

**3. Обробка некоректних даних (DomainException):**

- Запуск імпорту файлу з порушенням валідації (наприклад, порожній ISBN чи некоректний статус).

```bash
dotnet run --project src/Cli -- --file data/invalid.json
```

**Вивід:**

```text
Файл не знайдено: data/invalid.json
```
