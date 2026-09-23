### Runtime та публікація

Для перевірки крос-платформності проєкту використано два Runtime Identifier (RID):
* **win-x64** — для запуску застосунку на Windows x64;
* **linux-x64** — для запуску застосунку на Linux x64.

Для кожного RID перевірено три режими публікації: `self-contained`, `framework-dependent` та `single-file`.

| RID | Режим публікації | Розмір publish | Кількість файлів | Потрібен .NET Runtime | Опис середовища |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `win-x64` | **self-contained** | ~76.7 МБ | 192 | **Ні** | Включає `Cli.exe`, CoreCLR та збірки BCL. Автономний запуск на Windows. |
| `win-x64` | **framework-dependent** | ~0.2 МБ | 7 | **Так (.NET 10)** | Містить лише скомпільований код застосунку. Залежить від встановленого в системі .NET 10 Runtime. |
| `win-x64` | **single-file** | ~70.2 МБ | 3 | **Ні** | Усі системні бібліотеки та CoreCLR упаковані всередину одного монолітного файлу `Cli.exe`. |
| `linux-x64` | **self-contained** | ~78.8 МБ | 192 | **Ні** | Нативний виконуваний бінарник під Linux з власним рантаймом. Запускається без інсталяції .NET. |
| `linux-x64` | **framework-dependent** | ~0.2 МБ | 7 | **Так (.NET 10)** | Лише IL-байткод програми. Для роботи вимагає попередньо встановленого середовища .NET 10 на хості Linux. |
| `linux-x64` | **single-file** | ~72.4 МБ | 2 | **Ні** | Один виконуваний бінарний ELF-файл Linux (і `.pdb`), який містить у собі весь .NET Runtime. |
| `win-x64` | **trimmed** | ~25.4 МБ | 55 | **Ні** | Self-contained з увімкненим Trimming (`PublishTrimmed=true`): невикористаний код BCL вирізано, розмір суттєво зменшено. |
| `linux-x64` | **trimmed** | ~26.8 МБ | 55 | **Ні** | Self-contained з увімкненим Trimming (`PublishTrimmed=true`): невикористаний код видалено з Linux-рантайму. |
* **Self-contained**: пакує застосунок разом із середовищем виконання .NET Runtime (CLR) та бібліотеками BCL під конкретний RID. На цільовій системі (або в контейнері) встановлений .NET не потрібен, але розмір пакету сягає ~77–79 МБ.
* **Framework-dependent**: містить лише скомпільований код застосунку та сторонні бібліотеки. Займає мінімум місця (~0.2 МБ), проте вимагає наявності встановленого .NET 10 Runtime у хостовій системі чи контейнері.
* **Single-file**: об'єднує всі компоненти та runtime в один монолітний виконуваний файл, що зменшує кількість файлів у каталозі з 192 до 2–3 і спрощує розгортання програми.

#### Команди публікації під Windows:
```bash
# Self-contained
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -o publish/win-x64-self

# Framework-dependent
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false -o publish/win-x64-fdd

# Single-file
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -p:PublishSingleFile=true -o publish/win-x64-single

Публікація під Windows (win-x64)

Self-contained (автономна збірка):
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -o publish/win-x64-self

Перевірка розміру та кількості файлів у PowerShell:
$f = Get-ChildItem -Recurse -File publish/win-x64-self; Write-Host "Файлів:" $f.Count "\vert{} Розмір:" ([Math]::Round(($f | Measure-Object Length -Sum).Sum / 1MB, 2)) "MB"

Framework-dependent (залежна від .NET 10 Runtime):
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false -o publish/win-x64-fdd

Перевірка розміру:
$f = Get-ChildItem -Recurse -File publish/win-x64-fdd; Write-Host "Файлів:" $f.Count "\vert{} Розмір:" ([Math]::Round(($f | Measure-Object Length -Sum).Sum / 1KB, 2)) "KB"

Публікація під Linux (linux-x64)

Self-contained (автономна збірка для Linux):
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained true -o publish/linux-x64-self

Перевірка розміру:
$f = Get-ChildItem -Recurse -File publish/linux-x64-self; Write-Host "Файлів:" $f.Count "\vert{} Розмір:" ([Math]::Round(($f | Measure-Object Length -Sum).Sum / 1MB, 2)) "MB"

Framework-dependent для Linux:
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained false -o publish/linux-x64-fdd

Перевірка розміру:
$f = Get-ChildItem -Recurse -File publish/linux-x64-fdd; Write-Host "Файлів:" $f.Count "\vert{} Розмір:" ([Math]::Round(($f | Measure-Object Length -Sum).Sum / 1KB, 2)) "KB"

Прямий запуск скомпільованого бінарника Windows:
.\publish\win-x64-self\Cli.exe
