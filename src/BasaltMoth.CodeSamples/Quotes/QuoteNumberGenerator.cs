using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BasaltMoth.CodeSamples.Quotes;

/// <summary>
/// Atomically reserves the next per-company number using SQLite's upsert statement.
/// The statement participates in an active EF Core transaction when one exists.
/// </summary>
public sealed class QuoteNumberGenerator(SampleDbContext dbContext)
{
    public async Task<string> GenerateAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company id is required.", nameof(companyId));

        var connection = dbContext.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO "QuoteNumberSequences" ("CompanyId", "LastNumber")
                VALUES (@companyId, 1)
                ON CONFLICT ("CompanyId")
                DO UPDATE SET "LastNumber" = "LastNumber" + 1
                RETURNING "LastNumber";
                """;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@companyId";
            parameter.DbType = DbType.String;
            parameter.Value = companyId.ToString();
            command.Parameters.Add(parameter);

            var transaction = dbContext.Database.CurrentTransaction;
            if (transaction is not null)
                command.Transaction = transaction.GetDbTransaction();

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var number = Convert.ToInt64(result, System.Globalization.CultureInfo.InvariantCulture);
            return $"Q{number:D6}";
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }
}
