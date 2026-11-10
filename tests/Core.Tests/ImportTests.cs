using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Core.Import;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class ImportTests
{
    [Fact]
    // Скасування: заздалегідь скасований токен негайно зупиняє виконання
    public async Task ImportAsync_WhenAlreadyCancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new LendingService(new InMemoryLoanStore());
        var importer = new JsonCatalogImporter();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(importer, "dummy_path.json", ct: cts.Token));
    }

    [Fact]
    // Тимчасовий файл: перевірка валідації пошкоджених даних із гарантованим видаленням у finally
    public async Task ImportAsync_InvalidJsonRecord_ThrowsDomainException_AndCleansUpFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_catalog_{Guid.NewGuid():N}.json");
        var badJson = "[ { \"Id\": \"\", \"Isbn\": \"978-01\", \"Title\": \"Тест\" } ]";
        await File.WriteAllTextAsync(tempFile, badJson);

        var service = new LendingService(new InMemoryLoanStore());
        var importer = new JsonCatalogImporter();

        try
        {
            var ex = await Assert.ThrowsAsync<DomainException>(
                () => service.ImportAsync(importer, tempFile));

            Assert.Contains("Запис 1", ex.Message);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    // Додаткове завдання 2: Перевірка спрацювання саме тайм-ауту у зв'язаних токенах
    public async Task ImportAsync_LinkedTokens_TriggersTimeout()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"valid_catalog_{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(tempFile, "[ { \"Id\": \"BC-99\", \"Isbn\": \"978-01\", \"Title\": \"Книга\" } ]");

        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        // Чекаємо, щоб таймаут гарантовано спрацював
        await Task.Delay(60);

        var service = new LendingService(new InMemoryLoanStore());
        var importer = new JsonCatalogImporter();

        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.ImportAsync(importer, tempFile, ct: linkedCts.Token));

            Assert.True(timeoutCts.IsCancellationRequested);
            Assert.False(userCts.IsCancellationRequested);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}