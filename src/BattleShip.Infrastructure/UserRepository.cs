using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BattleShip.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace BattleShip.Infrastructure;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private const string Prefix = "v1";

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        Span<byte> salt = stackalloc byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        Span<byte> hash = stackalloc byte[KeySize];
        Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            hash,
            Iterations,
            HashAlgorithmName.SHA256);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        var parts = storedHash.Split('$');
        if (parts.Length == 4 && parts[0] == Prefix && int.TryParse(parts[1], out var iterations))
        {
            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        // Hỗ trợ tương thích ngược với SHA-256 hex (nếu có dữ liệu cũ)
        var legacyHex = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(legacyHex),
            Encoding.UTF8.GetBytes(storedHash));
    }
}

public static class DatabaseConfig
{
    public const string DefaultConnectionString =
        "Server=.\\SQLEXPRESS;Database=BattleShipDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public static string LoadConnectionString(string? basePath = null)
    {
        var directory = basePath ?? AppContext.BaseDirectory;
        var devSettings = Path.Combine(directory, "appsettings.Development.json");
        var appSettings = Path.Combine(directory, "appsettings.json");

        var targetFile = File.Exists(devSettings)
            ? devSettings
            : File.Exists(appSettings)
                ? appSettings
                : null;

        if (targetFile is null)
            return DefaultConnectionString;

        using var stream = File.OpenRead(targetFile);
        using var document = JsonDocument.Parse(stream);
        if (document.RootElement.TryGetProperty("ConnectionStrings", out var connSection) &&
            connSection.TryGetProperty("BattleShip", out var connElement) &&
            connElement.GetString() is { Length: > 0 } connectionString)
        {
            return connectionString;
        }

        return DefaultConnectionString;
    }
}

public sealed class UserRepository : IUserRepository
{
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 50;
    public const int MinPasswordLength = 6;

    private readonly string _connectionString;

    public UserRepository(string? connectionString = null)
    {
        _connectionString = string.IsNullOrWhiteSpace(connectionString)
            ? DatabaseConfig.LoadConnectionString()
            : connectionString;
    }

    public async Task<UserRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Username, PasswordHash, CreatedAt
            FROM dbo.Users
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapUser(reader);
    }

    public async Task<UserRecord?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        const string sql = """
            SELECT Id, Username, PasswordHash, CreatedAt
            FROM dbo.Users
            WHERE Username = @Username;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, MaxUsernameLength)
        {
            Value = username.Trim()
        });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapUser(reader);
    }

    public async Task<int> CreateAsync(
        string username,
        string passwordHash,
        DateTime createdAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        const string sql = """
            INSERT INTO dbo.Users (Username, PasswordHash, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Username, @PasswordHash, @CreatedAt);
            """;

        var utcTime = NormalizeUtc(createdAtUtc);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, MaxUsernameLength)
        {
            Value = username.Trim()
        });
        command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255)
        {
            Value = passwordHash
        });
        command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2)
        {
            Value = utcTime
        });

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    public async Task<AuthResult> RegisterAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var trimmedUsername = username?.Trim() ?? string.Empty;
        if (trimmedUsername.Length < MinUsernameLength || trimmedUsername.Length > MaxUsernameLength)
        {
            return new AuthResult(
                false,
                $"Tên đăng nhập phải từ {MinUsernameLength} đến {MaxUsernameLength} ký tự.");
        }

        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
        {
            return new AuthResult(
                false,
                $"Mật khẩu phải có ít nhất {MinPasswordLength} ký tự.");
        }

        var existing = await GetByUsernameAsync(trimmedUsername, cancellationToken);
        if (existing is not null)
        {
            return new AuthResult(false, "Tên đăng nhập đã tồn tại.");
        }

        var passwordHash = PasswordHasher.Hash(password);
        var createdAtUtc = DateTime.UtcNow;
        var userId = await CreateAsync(trimmedUsername, passwordHash, createdAtUtc, cancellationToken);
        var user = new UserRecord(userId, trimmedUsername, passwordHash, createdAtUtc);

        return new AuthResult(true, "Đăng ký tài khoản thành công.", user);
    }

    public async Task<AuthResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return new AuthResult(false, "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
        }

        var user = await GetByUsernameAsync(username.Trim(), cancellationToken);
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            return new AuthResult(false, "Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        return new AuthResult(true, "Đăng nhập thành công.", user);
    }

    private static UserRecord MapUser(SqlDataReader reader)
    {
        var id = reader.GetInt32(0);
        var username = reader.GetString(1);
        var passwordHash = reader.GetString(2);
        var createdAt = DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc);
        return new UserRecord(id, username, passwordHash, createdAt);
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
