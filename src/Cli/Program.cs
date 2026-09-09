using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;

var sysInfo = new
{
    App = "CrossApp",
    Student = "Ветвіцька Софія, ФЕІ-34",
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    ClrVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    BaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Бібліотека (Book, BookCopy, Reader, Loan)"
};

if (args.Contains("--json", StringComparer.OrdinalIgnoreCase))
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    Console.WriteLine(JsonSerializer.Serialize(sysInfo, options));
}
else
{
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine($"Студентка: {sysInfo.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription)   : {sysInfo.OSDescription}");
    Console.WriteLine($"ОС (Environment)     : {sysInfo.OSVersion}");
    Console.WriteLine($"Архітектура процесу  : {sysInfo.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)    : {sysInfo.ClrVersion}");
    Console.WriteLine($"Runtime              : {sysInfo.Runtime}");
    Console.WriteLine($"Каталог застосунку   : {sysInfo.BaseDirectory}");
    Console.WriteLine($"Поточний каталог     : {sysInfo.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {sysInfo.Domain}");
}