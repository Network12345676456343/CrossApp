using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Import;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async IAsyncEnumerable<BookCopy> ReadCopiesAsync(
        string path,
        IProgress<ImportProgress>? progress = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // 1. Пріоритет: миттєва реакція на вже скасований токен
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Шлях до файлу не може бути порожнім.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Файл не знайдено: {path}", path);

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        IAsyncEnumerable<BookCopyDto?> dtos;
        try
        {
            dtos = JsonSerializer.DeserializeAsyncEnumerable<BookCopyDto>(stream, SerializerOptions, ct);
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Файл '{Path.GetFileName(path)}' пошкоджено або він містить невалідний JSON.", ex);
        }

        var index = 0;
        await foreach (var dto in dtos.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            index++;

            if (dto is null)
                throw new DomainException($"Запис {index}: порожній елемент у каталозі.");

            BookCopy copy;
            try
            {
                copy = BookCopy.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {index}: {ex.Message}", ex);
            }

            progress?.Report(new ImportProgress(index, -1, 0));

            yield return copy;
        }
    }
}