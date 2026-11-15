using System.Data;
using BattleShip.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace BattleShip.Infrastructure;

public sealed class GameRepository : IGameRepository, ILeaderboardRepository
{
    public const string PlayerWonResult = "PlayerWon";
    public const string BotWonResult = "BotWon";
    public const string InProgressResult = "InProgress";

    private readonly string _connectionString;

    public GameRepository(string? connectionString = null)
    {
        _connectionString = string.IsNullOrWhiteSpace(connectionString)
            ? DatabaseConfig.LoadConnectionString()
            : connectionString;
    }

    public async Task<long> CreateGameAsync(
        int userId,
        string difficulty,
        DateTime startedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(difficulty);

        const string sql = """
            INSERT INTO dbo.Games (UserId, Difficulty, Result, StartedAt, FinishedAt, Shots)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Difficulty, @Result, @StartedAt, NULL, 0);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
        command.Parameters.Add(new SqlParameter("@Difficulty", SqlDbType.NVarChar, 20)
        {
            Value = difficulty.Trim()
        });
        command.Parameters.Add(new SqlParameter("@Result", SqlDbType.NVarChar, 20)
        {
            Value = InProgressResult
        });
        command.Parameters.Add(new SqlParameter("@StartedAt", SqlDbType.DateTime2)
        {
            Value = NormalizeUtc(startedAtUtc)
        });

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result);
    }

    public async Task CompleteGameAsync(
        long gameId,
        string result,
        int totalShots,
        DateTime finishedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(result);
        ArgumentOutOfRangeException.ThrowIfNegative(totalShots);

        const string sql = """
            UPDATE dbo.Games
            SET Result = @Result,
                Shots = @Shots,
                FinishedAt = @FinishedAt
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = gameId });
        command.Parameters.Add(new SqlParameter("@Result", SqlDbType.NVarChar, 20)
        {
            Value = result.Trim()
        });
        command.Parameters.Add(new SqlParameter("@Shots", SqlDbType.Int) { Value = totalShots });
        command.Parameters.Add(new SqlParameter("@FinishedAt", SqlDbType.DateTime2)
        {
            Value = NormalizeUtc(finishedAtUtc)
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<GameRecord?> GetByIdAsync(
        long gameId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, Difficulty, Result, StartedAt, FinishedAt, Shots
            FROM dbo.Games
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = gameId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapGame(reader);
    }

    public async Task<IReadOnlyList<GameRecord>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, Difficulty, Result, StartedAt, FinishedAt, Shots
            FROM dbo.Games
            WHERE UserId = @UserId
            ORDER BY StartedAt DESC;
            """;

        var games = new List<GameRecord>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            games.Add(MapGame(reader));
        }

        return games;
    }

    public async Task<IReadOnlyList<LeaderboardEntry>> GetTopPlayersAsync(
        int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topCount);

        const string sql = """
            SELECT TOP (@TopCount)
                u.Id AS UserId,
                u.Username,
                SUM(CASE WHEN g.Result IN (N'PlayerWon', N'Win') THEN 1 ELSE 0 END) AS Wins,
                COUNT(g.Id) AS TotalGames,
                ISNULL(MIN(CASE WHEN g.Result IN (N'PlayerWon', N'Win') AND g.Shots > 0 THEN g.Shots END), 0) AS BestShots
            FROM dbo.Users u
            LEFT JOIN dbo.Games g ON g.UserId = u.Id
            GROUP BY u.Id, u.Username
            ORDER BY Wins DESC,
                     CASE WHEN MIN(CASE WHEN g.Result IN (N'PlayerWon', N'Win') AND g.Shots > 0 THEN g.Shots END) IS NULL THEN 1 ELSE 0 END ASC,
                     BestShots ASC,
                     u.Username ASC;
            """;

        var entries = new List<LeaderboardEntry>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@TopCount", SqlDbType.Int) { Value = topCount });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rank = 1;
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new LeaderboardEntry(
                Rank: rank++,
                UserId: reader.GetInt32(0),
                Username: reader.GetString(1),
                Wins: reader.GetInt32(2),
                TotalGames: reader.GetInt32(3),
                BestShots: reader.GetInt32(4)));
        }

        return entries;
    }

    private static GameRecord MapGame(SqlDataReader reader)
    {
        var id = reader.GetInt64(0);
        var userId = reader.GetInt32(1);
        var difficulty = reader.GetString(2);
        var result = reader.GetString(3);
        var startedAt = DateTime.SpecifyKind(reader.GetDateTime(4), DateTimeKind.Utc);
        DateTime? finishedAt = reader.IsDBNull(5)
            ? null
            : DateTime.SpecifyKind(reader.GetDateTime(5), DateTimeKind.Utc);
        var shots = reader.GetInt32(6);

        return new GameRecord(id, userId, difficulty, result, startedAt, finishedAt, shots);
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
