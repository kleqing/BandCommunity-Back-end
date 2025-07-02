using BandCommunity.Domain.DTO.User;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.Interfaces;
using BandCommunity.Infrastructure.Data;
using BandCommunity.Shared.Constant;
using BandCommunity.Shared.Exceptions;
using BandCommunity.Shared.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BandCommunity.Repository.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public UserRepository(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<User?> GetUserByRefreshToken(string refreshToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
        return user;
    }

    public async Task<User?> UpdateBasicUserInformation(BasicInfoRequest request, Guid userId)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (existingUser == null)
        {
            return null;
        }

        existingUser.DateOfBirth = request.DateOfBirth.EnsureUtc();
        existingUser.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(existingUser);

        if (!result.Succeeded)
        {
            throw new GlobalException("Update", UserConstant.UserNotFound);
        }

        return existingUser;
    }
}