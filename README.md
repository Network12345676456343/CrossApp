Порівняння режимів публікації та середовищ виконання (Runtime)

Для перевірки роботи застосунку використано два цільові середовища: Windows x64 та Linux x64. Для кожного середовища застосунок можна опублікувати у двох режимах: self-contained та framework-dependent.

ОС / RID	Режим	Команда публікації	Потрібен .NET Runtime	Як запустити
Windows win-x64	Self-contained	dotnet publish -c Release -r win-x64 --self-contained true -o publish/win-x64-sc	Ні	.\publish\win-x64-sc\Cli.exe
Windows win-x64	Framework-dependent	dotnet publish -c Release -r win-x64 --self-contained false -o publish/win-x64-fd	Так, .NET 10	dotnet .\publish\win-x64-fd\Cli.dll
Linux linux-x64	Self-contained	dotnet publish -c Release -r linux-x64 --self-contained true -o publish/linux-x64-sc	Ні	./publish/linux-x64-sc/Cli
Linux linux-x64	Framework-dependent	dotnet publish -c Release -r linux-x64 --self-contained false -o publish/linux-x64-fd	Так, .NET 10	dotnet ./publish/linux-x64-fd/Cli.dll

Self-contained публікація містить застосунок разом із необхідним середовищем виконання .NET. Тому на цільовій системі окремо встановлювати .NET Runtime не потрібно. Недоліком є більший розмір публікації.

Framework-dependent публікація містить код застосунку та його залежності, але використовує .NET Runtime, встановлений у системі. Такий варіант займає значно менше місця, однак потребує встановленого відповідного .NET Runtime.

Публікація під Windows

Для створення self-contained версії під Windows x64 використовується команда:

dotnet publish Cli/Cli.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64-sc


Після завершення публікації програму можна запустити без встановлення .NET:

.\publish\win-x64-sc\Cli.exe


Для framework-dependent версії:

dotnet publish Cli/Cli.csproj -c Release -r win-x64 --self-contained false -o publish/win-x64-fd


Запуск:

dotnet .\publish\win-x64-fd\Cli.dll


У цьому випадку на Windows має бути встановлений .NET 10 Runtime.

Публікація під Linux

Для створення self-contained версії під Linux x64:

dotnet publish Cli/Cli.csproj -c Release -r linux-x64 --self-contained true -o publish/linux-x64-sc


Перед запуском виконуваному файлу потрібно надати права:

chmod +x publish/linux-x64-sc/Cli


Після цього застосунок запускається командою:

./publish/linux-x64-sc/Cli


Для framework-dependent версії:

dotnet publish Cli/Cli.csproj -c Release -r linux-x64 --self-contained false -o publish/linux-x64-fd


Запуск:

dotnet ./publish/linux-x64-fd/Cli.dll


Для цього на Linux має бути встановлений .NET 10 Runtime.

Контейнеризація Linux

Для запуску застосунку в ізольованому Linux-середовищі використовується багатоетапний Dockerfile. У першому етапі виконується компіляція та публікація застосунку, а в другому формується кінцевий образ для запуску.

Збірка Docker-образу:

docker build -t crossapp:linux .


Перевірка створеного образу:

docker images


Запуск контейнера:

docker run --rm crossapp:linux


Параметр --rm автоматично видаляє контейнер після завершення роботи програми.

Запуск контейнера у Windows

Docker Desktop дозволяє запускати Linux-контейнери безпосередньо у Windows. Після встановлення та запуску Docker Desktop необхідно виконати:

docker build -t crossapp:linux .


Після успішної збірки:

docker run --rm crossapp:linux


Таким чином, хоча хостовою системою є Windows, всередині контейнера застосунок працює у Linux-середовищі.

Запуск контейнера у Linux

На Linux після встановлення Docker виконуються ті самі команди:

docker build -t crossapp:linux .
docker run --rm crossapp:linux


Отже, один і той самий Linux Docker-образ можна використовувати як на Windows з Docker Desktop, так і безпосередньо на Linux-системі.

Підсумок

У роботі використано два RID:

win-x64 — для нативної публікації та запуску застосунку у Windows;

linux-x64 — для публікації та запуску застосунку у Linux.

Для звичайного запуску доступні два режими:

self-contained — не потребує встановленого .NET Runtime;

framework-dependent — потребує встановленого .NET 10 Runtime, але має значно менший розмір публікації.

Для контейнеризації використовується Linux-образ. Його можна зібрати та запустити як у Linux, так і у Windows через Docker Desktop.