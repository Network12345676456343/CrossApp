using System.Diagnostics;
using System.Text.Json;
using CrossApp.Core.Reports;

var baseDir = AppContext.BaseDirectory;
var jsonPath = Path.Combine(baseDir, "data", "sample.json");

if (!File.Exists(jsonPath))
{
    jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "sample.json");
}

List<StockLine> items;
if (File.Exists(jsonPath))
{
    var rawText = File.ReadAllText(jsonPath);
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    items = JsonSerializer.Deserialize<List<StockLine>>(rawText, options) ?? new();
}
else
{
    items = new();
}

var today = DateTime.Today;
var command = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal)) ?? "report-top";

switch (command)
{
    case "report-top":
        Console.WriteLine("=== ТОП-5 ЧИТАЧІВ ЗА КІЛЬКІСТЮ ВИДАЧ ===");
        PrintTop(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Console.WriteLine($"=== ПРОСТРОЧЕНІ ВИДАЧІ (DueDate < {today:yyyy-MM-dd}) ===");
        PrintLines(ReportService.LowStock(items, today));
        break;

    case "report-groups":
        Console.WriteLine("=== РОЗПОДІЛ ЗА ЧИТАЧАМИ (Method Syntax) ===");
        foreach (var g in ReportService.ByUnit(items))
        {
            Console.WriteLine($"  {g.Key,-15} позицій: {g.Count,3}  сумарно: {g.TotalQuantity,4}");
        }
        break;

    case "report-groups-query":
        Console.WriteLine("=== РОЗПОДІЛ ЗА ЧИТАЧАМИ (Query Syntax) ===");
        foreach (var g in ReportService.ByUnitQuerySyntax(items))
        {
            Console.WriteLine($"  {g.Key,-15} позицій: {g.Count,3}  сумарно: {g.TotalQuantity,4}");
        }
        break;

    case "report-sum":
        var t = ReportService.Totals(items, today);
        Console.WriteLine("=== ПІДСУМКОВА СТАТИСТИКА ===");
        Console.WriteLine($"  Позицій: {t.Items}, прострочено днів: {t.TotalOverdueDays}, середній запас: {t.AverageDaysRemaining:F2} дн., макс прострочення: {t.MaxOverdueDays} дн.");
        break;

    case "report-join":
        Console.WriteLine("=== ПРІОРИТЕТНІ ВИДАЧІ (JOIN) ===");
        var priorityReaders = new[] { "r-101", "r-102" };
        foreach (var j in ReportService.ActiveLoansJoin(items, priorityReaders))
        {
            Console.WriteLine($"  {j.LoanId,-8} Читач: {j.ReaderId,-8} Примірник: {j.BookCopyId,-8} Термін: {j.DueDate:yyyy-MM-dd}");
        }
        break;

    case "demo-deferred":
        RunDeferredDemo(items);
        break;

    case "benchmark":
        RunStopwatchBenchmark();
        break;

    default:
        Console.WriteLine("report-top | report-low | report-groups | report-groups-query | report-sum | report-join | demo-deferred | benchmark");
        break;
}

static void PrintLines(IReadOnlyList<StockLine> lines)
{
    Console.WriteLine($"  {"ID",-8} {"Читач",-8} {"Примірник",-10} {"Термін",-12} {"Прострочено",12}");
    Console.WriteLine($"  {new string('-', 55)}");
    foreach (var l in lines)
    {
        Console.WriteLine($"  {l.Id,-8} {l.ReaderId,-8} {l.BookCopyId,-10} {l.DueDate:yyyy-MM-dd} {l.DaysOverdue,8} дн.");
    }
}

static void PrintTop(IReadOnlyList<TopReaderLine> lines)
{
    Console.WriteLine($"  {"Читач",-10} {"Активних видач",14}");
    Console.WriteLine($"  {new string('-', 28)}");
    foreach (var l in lines)
    {
        Console.WriteLine($"  {l.ReaderId,-10} {l.ActiveLoansCount,14}");
    }
}

static void RunDeferredDemo(IEnumerable<StockLine> existingItems)
{
    Console.WriteLine("--- ДЕМОНСТРАЦІЯ ВІДКЛАДЕНОГО ВИКОНАННЯ ---");
    var source = existingItems.ToList();

    var query = source.Where(l => l.DueDate < DateTime.Today);
    Console.WriteLine($"1. До змін у джерелі: {query.Count()}");

    source.Add(new StockLine("tmp-999", "r-999", "cp-999", DateTime.Today.AddDays(-5), 5));
    Console.WriteLine($"2. Після додавання в source: {query.Count()} (число змінилось, бо Count() переобчислив запит)");

    var snapshot = query.ToList();
    source.Clear();
    Console.WriteLine($"3. Джерело очищене, але у знімку snapshot: {snapshot.Count} (не змінилось)");
}

static void RunStopwatchBenchmark()
{
    Console.WriteLine("=== ГЕНЕРАЦІЯ 100 000 ЗАПИСІВ ДЛЯ ТЕСТУВАННЯ ===");
    var random = new Random(42);
    var dataset = Enumerable.Range(1, 100_000)
        .Select(i => new StockLine(
            Id: $"loan-{i}",
            ReaderId: $"reader-{random.Next(1, 1000)}",
            BookCopyId: $"copy-{random.Next(1, 5000)}",
            DueDate: DateTime.Today.AddDays(random.Next(-60, 30)),
            DaysOverdue: random.Next(0, 60)))
        .ToList();

    Console.WriteLine($"Згенеровано {dataset.Count:N0} записів. Запуск вимірювання...\n");

    var sw = new Stopwatch();

    // 1. Потоковий конвеєр без проміжного ToList
    sw.Restart();
    var resultLazy = dataset
        .Where(x => x.DueDate < DateTime.Today)
        .OrderByDescending(x => x.DaysOverdue)
        .Select(x => x.Id)
        .Take(50)
        .ToList();
    sw.Stop();
    var timeLazy = sw.ElapsedMilliseconds;

    // 2. Конвеєр з проміжним ToList у середині ланцюжка
    sw.Restart();
    var resultEager = dataset
        .Where(x => x.DueDate < DateTime.Today)
        .ToList()
        .OrderByDescending(x => x.DaysOverdue)
        .Select(x => x.Id)
        .Take(50)
        .ToList();
    sw.Stop();
    var timeEager = sw.ElapsedMilliseconds;

    Console.WriteLine("--- РЕЗУЛЬТАТИ ВИМІРЮВАННЯ (Stopwatch) ---");
    Console.WriteLine($"1. Без проміжного ToList() : {timeLazy} мс");
    Console.WriteLine($"2. З проміжним ToList()    : {timeEager} мс");
    Console.WriteLine($"Різниця: {timeEager - timeLazy} мс на користь лінивого конвеєра.");
}