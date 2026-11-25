using BattleShip.Domain;

namespace BattleShip.Application.Interfaces;

public sealed record ShotHistoryEntry(
    string Actor,
    Cell Target,
    ShotResult Result,
    DateTime FiredAtUtc);

public sealed record ShotRecord(
    long Id,
    long GameId,
    string Actor,
    int RowIndex,
    int ColumnIndex,
    string Result,
    DateTime FiredAtUtc);

public interface IShotRepository
{
    Task<long> AddShotAsync(
        long gameId,
        string actor,
        Cell target,
        ShotResult result,
        DateTime firedAtUtc,
        CancellationToken cancellationToken = default);

    Task AddShotsBatchAsync(
        long gameId,
        IEnumerable<ShotHistoryEntry> shots,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShotRecord>> GetShotsByGameIdAsync(
        long gameId,
        CancellationToken cancellationToken = default);
}
