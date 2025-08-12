using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Application.Contracts;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
}
