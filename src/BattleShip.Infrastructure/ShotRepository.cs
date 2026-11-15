using System.Data;
using BattleShip.Application.Interfaces;
using BattleShip.Domain;
using Microsoft.Data.SqlClient;

namespace BattleShip.Infrastructure;

public sealed class ShotRepository : IShotRepository
{
    private readonly string _connectionString;

    public ShotRepository(string? connectionString = null)
    {
        _connectionString = string.IsNullOrWhiteSpace(connectionString)
            ? DatabaseConfig.LoadConnectionString()
            : connectionString;
    }

    public async Task<long> AddShotAsync(
        long gameId,
        string actor,
        Cell target,
        ShotResult result,
        DateTime firedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);

        const string sql = """
            INSERT INTO dbo.Shots (GameId, Actor, RowIndex, ColumnIndex, Result, FiredAt)
            OUTPUT INSERTED.Id
            VALUES (@GameId, @Actor, @RowIndex, @ColumnIndex, @Result, @FiredAt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@GameId", SqlDbType.BigInt) { Value = gameId });
        command.Parameters.Add(new SqlParameter("@Actor", SqlDbType.NVarChar, 10)
        {
            Value = actor.Trim()
        });
        command.Parameters.Add(new SqlParameter("@RowIndex", SqlDbType.Int) { Value = target.Row });
        command.Parameters.Add(new SqlParameter("@ColumnIndex", SqlDbType.Int) { Value = target.Column });
        command.Parameters.Add(new SqlParameter("@Result", SqlDbType.NVarChar, 20)
        {
            Value = result.ToString()
        });
        command.Parameters.Add(new SqlParameter("@FiredAt", SqlDbType.DateTime2)
        {
            Value = NormalizeUtc(firedAtUtc)
        });

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(scalar);
    }

    public async Task AddShotsBatchAsync(
        long gameId,
        IEnumerable<ShotHistoryEntry> shots,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(shots);

        const string sql = """
            INSERT INTO dbo.Shots (GameId, Actor, RowIndex, ColumnIndex, Result, FiredAt)
            VALUES (@GameId, @Actor, @RowIndex, @ColumnIndex, @Result, @FiredAt);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var shot in shots)
            {
                await using var command = new SqlCommand(sql, connection, transaction);
                command.Parameters.Add(new SqlParameter("@GameId", SqlDbType.BigInt) { Value = gameId });
                command.Parameters.Add(new SqlParameter("@Actor", SqlDbType.NVarChar, 10)
                {
                    Value = shot.Actor.Trim()
                });
                command.Parameters.Add(new SqlParameter("@RowIndex", SqlDbType.Int) { Value = shot.Target.Row });
                command.Parameters.Add(new SqlParameter("@ColumnIndex", SqlDbType.Int) { Value = shot.Target.Column });
                command.Parameters.Add(new SqlParameter("@Result", SqlDbType.NVarChar, 20)
                {
                    Value = shot.Result.ToString()
                });
                command.Parameters.Add(new SqlParameter("@FiredAt", SqlDbType.DateTime2)
                {
                    Value = NormalizeUtc(shot.FiredAtUtc)
                });

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<ShotRecord>> GetShotsByGameIdAsync(
        long gameId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, GameId, Actor, RowIndex, ColumnIndex, Result, FiredAt
            FROM dbo.Shots
            WHERE GameId = @GameId
            ORDER BY FiredAt ASC, Id ASC;
            """;

        var records = new List<ShotRecord>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@GameId", SqlDbType.BigInt) { Value = gameId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            records.Add(new ShotRecord(
                Id: reader.GetInt64(0),
                GameId: reader.GetInt64(1),
                Actor: reader.GetString(2),
                RowIndex: reader.GetInt32(3),
                ColumnIndex: reader.GetInt32(4),
                Result: reader.GetString(5),
                FiredAtUtc: DateTime.SpecifyKind(reader.GetDateTime(6), DateTimeKind.Utc)));
        }

        return records;
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
