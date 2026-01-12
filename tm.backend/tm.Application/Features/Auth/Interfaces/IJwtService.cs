using tm.Domain.Entities;

namespace tm.Application.Features.Auth.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}

