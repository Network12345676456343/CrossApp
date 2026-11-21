namespace Core.Storage;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

public sealed class FileLoanStore(string path) : ILoanStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _path = !string.IsNullOrWhiteSpace(path)
        ? path
        : throw new ArgumentException("Шлях до файлу сховища не може бути порожнім.", nameof(path));

    private readonly Dictionary<string, Loan> _cache = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (File.Exists(_path))
        {
            try
            {
                var json = File.ReadAllText(_path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var dtos = JsonSerializer.Deserialize<List<LoanDto>>(json, Options) ?? [];
                    _cache.Clear();
                    foreach (var dto in dtos)
                    {
                        var loan = Loan.FromDto(dto);
                        _cache[loan.Id] = loan;
                    }
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Некоректний формат JSON у файлі '{_path}': {ex.Message}", ex);
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var dtos = _cache.Values.Select(l => l.ToDto()).ToList();
        var tmpPath = _path + ".tmp";

        File.WriteAllText(tmpPath, JsonSerializer.Serialize(dtos, Options));
        File.Move(tmpPath, _path, overwrite: true);
    }

    public IReadOnlyList<Loan> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Loan? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        EnsureLoaded();

        if (_cache.ContainsKey(loan.Id))
            throw new InvalidOperationException($"Позичка з Id='{loan.Id}' уже існує у файлі.");

        _cache.Add(loan.Id, loan);
        Flush();
    }

    public void Update(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        EnsureLoaded();

        _cache[loan.Id] = loan;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;

        Flush();
        return true;
    }

    public IReadOnlyList<Loan> Find(Func<Loan, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }
}