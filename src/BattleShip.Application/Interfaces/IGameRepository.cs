namespace BattleShip.Application.Interfaces;

public sealed record GameRecord(
    long Id,
    int UserId,
    string Difficulty,
    string Result,
    DateTime StartedAtUtc,
    DateTime? FinishedAtUtc,
    int Shots);

public interface IGameRepository
{
    Task<long> CreateGameAsync(
        int userId,
        string difficulty,
        DateTime startedAtUtc,
        CancellationToken cancellationToken = default);

    Task CompleteGameAsync(
        long gameId,
        string result,
        int totalShots,
        DateTime finishedAtUtc,
        CancellationToken cancellationToken = default);

    Task<GameRecord?> GetByIdAsync(
        long gameId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameRecord>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
