using System.Data;
using Microsoft.Data.SqlClient;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT Id, Username, PasswordHash, Email, Role, CreatedAt, IsActive FROM Users WHERE Id=@Id";
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var rdr = await cmd.ExecuteReaderAsync(ct);
        if (!await rdr.ReadAsync(ct)) return null;
        return Map(rdr);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        const string sql = "SELECT Id, Username, PasswordHash, Email, Role, CreatedAt, IsActive FROM Users WHERE Username=@Username";
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Username", username);
        using var rdr = await cmd.ExecuteReaderAsync(ct);
        if (!await rdr.ReadAsync(ct)) return null;
        return Map(rdr);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        const string sql = "SELECT Id, Username, PasswordHash, Email, Role, CreatedAt, IsActive FROM Users WHERE Email=@Email";
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", email);
        using var rdr = await cmd.ExecuteReaderAsync(ct);
        if (!await rdr.ReadAsync(ct)) return null;
        return Map(rdr);
    }

    public async Task<int> CreateAsync(User user, CancellationToken ct = default)
    {
        const string sql = @"INSERT INTO Users (Username, PasswordHash, Email, Role, CreatedAt, IsActive)
VALUES (@Username, @PasswordHash, @Email, @Role, @CreatedAt, @IsActive);
SELECT SCOPE_IDENTITY();";
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@Role", user.Role);
        cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        const string sql = @"UPDATE Users SET Username=@Username, PasswordHash=@PasswordHash, Email=@Email, Role=@Role, IsActive=@IsActive WHERE Id=@Id";
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", user.Id);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@Role", user.Role);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    private static User Map(SqlDataReader rdr) => new()
    {
        Id = rdr.GetInt32(0),
        Username = rdr.GetString(1),
        PasswordHash = rdr.GetString(2),
        Email = rdr.GetString(3),
        Role = rdr.GetString(4),
        CreatedAt = rdr.GetDateTime(5),
        IsActive = rdr.GetBoolean(6)
    };
}
