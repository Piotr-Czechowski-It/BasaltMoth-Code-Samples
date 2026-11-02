using System.Collections.Concurrent;
using BasaltMoth.CodeSamples.Quotes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BasaltMoth.CodeSamples.Tests.Quotes;

public sealed class QuoteNumberGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_IncrementsSequencePerCompany()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SampleDbContext>().UseSqlite(connection).Options;

        await using var db = new SampleDbContext(options);
         
        await db.Database.EnsureCreatedAsync();

        var generator = new QuoteNumberGenerator(db);

        var firstCompany = Guid.NewGuid();
        var secondCompany = Guid.NewGuid();

        Assert.Equal("Q000001", await generator.GenerateAsync(firstCompany));
        Assert.Equal("Q000002", await generator.GenerateAsync(firstCompany));
        Assert.Equal("Q000001", await generator.GenerateAsync(secondCompany));
    }

    [Fact]
    public async Task GenerateAsync_UsesCurrentEfTransaction()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SampleDbContext>().UseSqlite(connection).Options;
        
        await using var db = new SampleDbContext(options);

        await db.Database.EnsureCreatedAsync();

        var generator = new QuoteNumberGenerator(db);
        var companyId = Guid.NewGuid();

        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            Assert.Equal("Q000001", await generator.GenerateAsync(companyId));
            await transaction.RollbackAsync();
        }

        Assert.Equal("Q000001", await generator.GenerateAsync(companyId));
    }

    [Fact]
    public async Task GenerateAsync_ConcurrentCallsReserveDistinctNumbers()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"quote-sequences-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={databasePath};Default Timeout=30;Pooling=False";
        try
        {
            var options = new DbContextOptionsBuilder<SampleDbContext>().UseSqlite(connectionString).Options;
            
            await using (var setup = new SampleDbContext(options))
                await setup.Database.EnsureCreatedAsync();

            var companyId = Guid.NewGuid();
            var values = new ConcurrentBag<string>();
            
            await Parallel.ForEachAsync(Enumerable.Range(0, 16),
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (_, cancellationToken) =>
                {
                    await using var db = new SampleDbContext(options);
                    values.Add(await new QuoteNumberGenerator(db).GenerateAsync(companyId, cancellationToken));
                });

            Assert.Equal(16, values.Distinct().Count());
            Assert.Equal(Enumerable.Range(1, 16).Select(number => $"Q{number:D6}").Order(), values.Order());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task GenerateAsync_RejectsEmptyCompanyId()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SampleDbContext>().UseSqlite(connection).Options;
        await using var db = new SampleDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var generator = new QuoteNumberGenerator(db);

        await Assert.ThrowsAsync<ArgumentException>(() => generator.GenerateAsync(Guid.Empty));
    }
}
