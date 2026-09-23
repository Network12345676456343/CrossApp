### Контейнеризація (Docker)

Для крос-платформного розгортання налаштовано кілька конфігурацій Docker:

| Варіант контейнера | Базовий образ (Base Image) | Режим публікації | Орієнтовний розмір | Призначення |
| :--- | :--- | :--- | :--- | :--- |
| **Linux Standard** (`Dockerfile`) | `dotnet/runtime:10.0` | Framework-dependent | ~180 MB | Стандартне середовище для розробки та тестування. |
| **Linux Self-contained** | `dotnet/runtime-deps:10.0` | Self-contained Single-File | ~110 MB | Повна автономність без потреби встановленого .NET SDK/Runtime. |
| **Linux Trimmed** (`Dockerfile.trimmed`) | `dotnet/runtime-deps:10.0` | Self-contained (PublishTrimmed) | **~35 MB** | **Ультралегкий production-образ з видаленням мертвого системного коду.** |
| **Windows Nano Server** (`Dockerfile.windows`) | `dotnet/runtime:10.0-nanoserver` | Framework-dependent | ~300 MB | Запуск у нативному Windows-середовищі контейнерів. |

#### Збірка та запуск Trimmed-контейнера:
```bash
# Збірка
docker build -f Dockerfile.trimmed -t crossapp:trimmed .

# Запуск
docker run --rm crossapp:trimmed
