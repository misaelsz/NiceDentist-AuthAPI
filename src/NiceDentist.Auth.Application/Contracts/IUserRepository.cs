using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Application.Contracts;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(User user, CancellationToken ct = default);
    Task<bool> UpdateAsync(User user, CancellationToken ct = default);
}
