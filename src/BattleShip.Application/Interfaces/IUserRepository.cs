namespace BattleShip.Application.Interfaces;

public sealed record UserRecord(
    int Id,
    string Username,
    string PasswordHash,
    DateTime CreatedAtUtc);

public sealed record AuthResult(
    bool Success,
    string Message,
    UserRecord? User = null);

public interface IUserRepository
{
    Task<UserRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<UserRecord?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(string username, string passwordHash, DateTime createdAtUtc, CancellationToken cancellationToken = default);

    Task<AuthResult> RegisterAsync(string username, string password, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}
