Runtime

Для перевірки кросплатформності проєкту використано два Runtime Identifier (RID):

win-x64 — для запуску застосунку на Windows x64;

linux-x64 — для запуску застосунку на Linux x64.

Для кожного RID перевірено два режими публікації: self-contained та framework-dependent.

RID	Режим публікації	Розмір publish	Кількість файлів	Потрібен .NET Runtime
win-x64	self-contained	78 MB	197	Ні
win-x64	framework-dependent	237 KB	10	Так (.NET 10)
linux-x64	self-contained	80 MB	197	Ні
linux-x64	framework-dependent	153 KB	10	Так (.NET 10)

Self-contained — застосунок публікується разом із необхідним .NET Runtime та бібліотеками. Тому на цільовій системі не потрібно додатково встановлювати .NET Runtime. Недоліком є більший розмір publish-директорії.

Framework-dependent — публікується лише застосунок та його залежності, без .NET Runtime. Такий варіант має значно менший розмір, але для запуску на цільовій системі необхідно мати встановлений .NET 10 Runtime.

Публікація під Windows:

dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r win-x64 --self-contained false


Публікація під Linux:

dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
dotnet publish src/Cli -c Release -r linux-x64 --self-contained false


Self-contained версію Windows можна запустити без dotnet run:

.\src\Cli\bin\Release\net10.0\win-x64\publish\Cli.exe


Self-contained версію Linux:

chmod +x src/Cli/bin/Release/net10.0/linux-x64/publish/Cli
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli


Framework-dependent версії запускаються через встановлений .NET Runtime:

dotnet ./src/Cli/bin/Release/net10.0/win-x64/publish/Cli.dll
dotnet ./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli.dll


Таким чином, self-contained забезпечує автономний запуск без встановлення .NET, тоді як framework-dependent дозволяє отримати значно менший пакет за умови наявності .NET 10 Runtime у системі.
framework-dependent — потребує встановленого .NET 10 Runtime, але має значно менший розмір публікації.

Для контейнеризації використовується Linux-образ. Його можна зібрати та запустити як у Linux, так і у Windows через Docker Desktop.
