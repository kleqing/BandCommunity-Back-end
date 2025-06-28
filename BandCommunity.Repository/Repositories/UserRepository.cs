using BandCommunity.Domain.Entities;
using BandCommunity.Domain.Interfaces;
using BandCommunity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BandCommunity.Repository.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByRefreshToken(string refreshToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
        return user;
    }
}