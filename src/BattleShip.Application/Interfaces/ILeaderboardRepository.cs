namespace BattleShip.Application.Interfaces;

public sealed record LeaderboardEntry(
    int Rank,
    int UserId,
    string Username,
    int Wins,
    int TotalGames,
    int BestShots);

public interface ILeaderboardRepository
{
    Task<IReadOnlyList<LeaderboardEntry>> GetTopPlayersAsync(
        int topCount = 10,
        CancellationToken cancellationToken = default);
}
