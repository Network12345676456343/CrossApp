### Runtime та публікація

Для перевірки крос-платформності проєкту використано два Runtime Identifier (RID):
* **win-x64** — для запуску застосунку на Windows x64;
* **linux-x64** — для запуску застосунку на Linux x64.

Для кожного RID перевірено режими публікації: `self-contained`, `framework-dependent` та додатковий режим `single-file`.

| RID | Режим публікації | Розмір publish | Кількість файлів | Потрібен .NET Runtime | Опис середовища |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `win-x64` | **self-contained** | ~76.7 МБ | 192 | **Ні** | Містить у собі повну копію CoreCLR та системні збірки BCL. Автономний запуск на Windows. |
| `win-x64` | **framework-dependent** | ~0.2 МБ | 7 | **Так (.NET 10)** | Містить лише скомпільований код застосунку. Залежить від встановленого в системі .NET 10 Runtime. |
| `win-x64` | **single-file** | ~70.2 МБ | 3 | **Ні** | Усі системні бібліотеки та CoreCLR упаковані всередину одного монолітного файлу Cli.exe. |
| `linux-x64` | **self-contained** | ~78.8 МБ | 192 | **Ні** | Нативний виконуваний бінарник під Linux з власним рантаймом. Запускається без інсталяції .NET. |
| `linux-x64` | **framework-dependent** | ~0.2 МБ | 7 | **Так (.NET 10)** | Лише IL-байткод програми. Для роботи вимагає попередньо встановленого середовища .NET 10 на хості Linux. |

* **Self-contained**: пакує застосунок разом із середовищем виконання .NET Runtime (CLR) та бібліотеками BCL під конкретний RID. На цільовій системі (або в контейнері) встановлений .NET не потрібен, але розмір пакету сягає ~77–79 МБ.
* **Framework-dependent**: містить лише скомпільований код застосунку та сторонні бібліотеки. Займає мінімум місця (~0.2 МБ), проте вимагає наявності встановленого .NET 10 Runtime у хостовій системі чи контейнері.
* **Single-file**: об'єднує всі компоненти та runtime в один монолітний виконуваний файл, що зменшує кількість файлів у каталозі з 192 до 3 і спрощує розгортання програми.

#### Команди публікації під Windows:
```bash
# Self-contained
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -o publish/win-x64-self

# Framework-dependent
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false -o publish/win-x64-fdd

# Single-file
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -p:PublishSingleFile=true -o publish/win-x64-single
