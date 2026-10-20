namespace Core.Storage;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

public sealed class FileLoanStore(string path) : ILoanStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path = path ?? throw new ArgumentNullException(nameof(path));
    private readonly Dictionary<string, Loan> _cache = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (File.Exists(_path))
        {
            var json = File.ReadAllText(_path);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var dtos = JsonSerializer.Deserialize<List<LoanDto>>(json, Options) ?? [];
                foreach (var dto in dtos)
                {
                    var loan = Loan.FromDto(dto);
                    _cache[loan.Id] = loan;
                }
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
        File.WriteAllText(_path, JsonSerializer.Serialize(dtos, Options));
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