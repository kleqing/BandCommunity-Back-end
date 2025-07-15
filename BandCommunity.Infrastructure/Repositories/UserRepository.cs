using BandCommunity.Domain.Entities;
using BandCommunity.Domain.Interfaces;
using BandCommunity.Domain.Models.User;
using BandCommunity.Infrastructure.Data;
using BandCommunity.Shared.Constant;
using BandCommunity.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BandCommunity.Infrastructure.Repositories;

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

    public async Task<User?> GetCurrentUserInformation(BasicInfoRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        
        if (user == null)
        {
            throw new GlobalException(UserConstant.UserNotFound);
        }

        return user;
    }
}