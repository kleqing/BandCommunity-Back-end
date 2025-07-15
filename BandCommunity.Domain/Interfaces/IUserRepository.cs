using BandCommunity.Domain.Entities;
using BandCommunity.Domain.Models.User;

namespace BandCommunity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshToken(string refreshToken);
    Task<User?> GetCurrentUserInformation(BasicInfoRequest request);
}