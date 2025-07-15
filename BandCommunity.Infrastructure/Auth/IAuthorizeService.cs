using System.Security.Claims;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace BandCommunity.Infrastructure.Auth;

public interface IAuthorizeService
{
    Task<User> CreateAccount(RegisterRequest request);
    Task<User> Login(LoginRequest request);
    Task InitiatePasswordReset(string email);
    Task<bool> VerifyPasswordResetToken(string token);
    Task<IdentityResult> ResetPassword(ResetPasswordRequest request);
    Task ResendEmailConfirmation(User user);
    Task RefreshToken(string? token);
    Task Logout(User user);
}