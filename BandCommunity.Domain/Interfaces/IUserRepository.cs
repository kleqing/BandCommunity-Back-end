using BandCommunity.Domain.DTO.User;
using BandCommunity.Domain.Entities;

namespace BandCommunity.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshToken(string refreshToken);
    Task<User?> UpdateBasicUserInformation(BasicInfoRequest user, Guid userId);
}