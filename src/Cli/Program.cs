using System.Text.Encodings.Web;
using System.Text.Json;
using Core;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Length > 0 && args[0] == "--json")
{
    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };
    Console.WriteLine(JsonSerializer.Serialize(report, options));
}
else
{
    Console.WriteLine("CrossApp – інформація про середовище");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС             : {report.OsDescription}");
    Console.WriteLine($"Runtime        : {report.FrameworkDescription}");
    Console.WriteLine($"Архітектура    : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено): {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    Console.WriteLine($"Каталог        : {report.BaseDirectory}");
    Console.WriteLine($"Цільова збірка : {report.BuildNote}");
}