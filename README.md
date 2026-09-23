Ось чистий текст без форматування Markdown (без грат, зірочок і блоків коду), щоб зручно було скопіювати і вставити:

Runtime та публікація

Для перевірки крос-платформності проєкту використано два Runtime Identifier (RID):

win-x64 — для запуску застосунку на Windows x64;

linux-x64 — для запуску застосунку на Linux x64.

Для кожного RID перевірено три режими публікації: self-contained, framework-dependent та single-file.

Порівняльна таблиця:
RID | Режим публікації | Розмір publish | Кількість файлів | Потрібен .NET Runtime | Опис середовища
win-x64 | self-contained | ~76.7 МБ | 192 | Ні | Включає Cli.exe, CoreCLR та збірки BCL. Автономний запуск на Windows.
win-x64 | framework-dependent | ~0.2 МБ | 7 | Так (.NET 10) | Містить лише скомпільований код застосунку. Залежить від встановленого в системі .NET 10 Runtime.
win-x64 | single-file | ~70.2 МБ | 3 | Ні | Усі системні бібліотеки та CoreCLR упаковані всередину одного монолітного файлу Cli.exe.
linux-x64 | self-contained | ~78.8 МБ | 192 | Ні | Нативний виконуваний бінарник під Linux з власним рантаймом. Запускається без інсталяції .NET.
linux-x64 | framework-dependent | ~0.2 МБ | 7 | Так (.NET 10) | Лише IL-байткод програми. Для роботи вимагає попередньо встановленого середовища .NET 10 на хості Linux.
linux-x64 | single-file | ~72.4 МБ | 2 | Ні | Один виконуваний бінарний ELF-файл Linux (і .pdb), який містить у собі весь .NET Runtime.

Опис режимів:

Self-contained: пакує застосунок разом із середовищем виконання .NET Runtime (CLR) та бібліотеками BCL під конкретний RID. На цільовій системі (або в контейнері) встановлений .NET не потрібен, але розмір пакету сягає ~77–79 МБ.

Framework-dependent: містить лише скомпільований код застосунку та сторонні бібліотеки. Займає мінімум місця (~0.2 МБ), проте вимагає наявності встановленого .NET 10 Runtime у хостовій системі чи контейнері.

Single-file: об'єднує всі компоненти та runtime в один монолітний виконуваний файл, що зменшує кількість файлів у каталозі з 192 до 2–3 і спрощує розгортання програми.

Команди публікації під Windows:
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -o publish/win-x64-self
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false -o publish/win-x64-fdd
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -p:PublishSingleFile=true -o publish/win-x64-single

Команди публікації під Linux:
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained true -o publish/linux-x64-self
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained false -o publish/linux-x64-fdd
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64-single

Запуск застосунків:
Self-contained Windows: .\publish\win-x64-self\Cli.exe
Single-file Windows: .\publish\win-x64-single\Cli.exe
Self-contained Linux: chmod +x ./publish/linux-x64-self/Cli && ./publish/linux-x64-self/Cli
Single-file Linux: chmod +x ./publish/linux-x64-single/Cli && ./publish/linux-x64-single/Cli
Framework-dependent Windows: dotnet ./publish/win-x64-fdd/Cli.dll
Framework-dependent Linux: dotnet ./publish/linux-x64-fdd/Cli.dll

Контейнеризація (Docker):
Для контейнеризації використовується Linux-образ. Його можна зібрати та запустити як у Linux, так і у Windows через Docker Desktop.

Таблиця варіантів контейнерів:
Варіант контейнера | Базовий образ (Base Image) | Режим застосунку | Розмір контейнера | Призначення та особливості

Linux Framework-dependent | mcr.microsoft.com/dotnet/runtime:10.0 | Framework-dependent | ~180 MB | Класичний контейнер. Застосунок використовує спільний runtime, встановлений в образі.

Linux Self-contained | mcr.microsoft.com/dotnet/runtime-deps:10.0 | Self-contained | ~110 MB | Контейнер без .NET SDK/Runtime; містить лише системні бібліотеки Linux (glibc).

Linux Single-File | mcr.microsoft.com/dotnet/runtime-deps:10.0 | Self-contained Single-File | ~105 MB | Один скомпільований бінарник усередині легкого базового Linux-образу.

Linux Chiseled / Alpine | mcr.microsoft.com/dotnet/nightly/runtime-deps:10.0-chiseled | Self-contained (Trimmed) | ~40 MB | Ультралегкий захищений образ для production без оболонки shell і зайвих пакетів.

Команди для Docker:
Збірка образу: docker build -t crossapp:linux .
Запуск контейнера: docker run --rm crossapp:linux
Запуск контейнера з прапорцем JSON: docker run --rm crossapp:linux --json
