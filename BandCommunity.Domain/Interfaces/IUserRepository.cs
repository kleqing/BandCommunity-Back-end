using BandCommunity.Domain.Entities;

namespace BandCommunity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshToken(string refreshToken);
}