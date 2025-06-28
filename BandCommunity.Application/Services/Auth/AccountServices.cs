using System.Net;
using System.Security.Claims;
using BandCommunity.Domain.DTO.Auth;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.Enums;
using BandCommunity.Domain.Interfaces;
using BandCommunity.Infrastructure.Auth;
using BandCommunity.Shared.Constant;
using BandCommunity.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using StackExchange.Redis;

namespace BandCommunity.Application.Services.Auth;

public class AccountServices : IAccountServices
{
    private readonly IAuthTokenProcess _authTokenProcess;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IDatabase _redisDatabase;
    private readonly TimeSpan _tokenExpiryTime;
    
    private const string RedisPasswordResetPrefix = "password_reset:";

    public AccountServices(
        IAuthTokenProcess authTokenProcess,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserRepository userRepository,
        IEmailSender emailSender,
        IDatabase redisDatabase)
    {
        _authTokenProcess = authTokenProcess;
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _redisDatabase = redisDatabase;
        _tokenExpiryTime = TimeSpan.FromMinutes(int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "15"));
    }

    public async Task LoginWithGoogle(ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal == null)
        {
            throw new GlobalException("Claims principal cannot be null", LoginConstant.ClaimPrincipalFailed);
        }
        
        var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
            throw new GlobalException("Email claim not found in claims principal", LoginConstant.ClaimPrincipalFailed);
        }
        
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            var newUser = new User
            {
                Email = email,
                UserName = email.Split('@')[0],
                FirstName = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                EmailConfirmed = true,
                ProfilePictureUrl = claimsPrincipal.FindFirstValue("profile_picture") ?? string.Empty,
            };
            
            var result = _userManager.CreateAsync(newUser);
            await _userManager.AddToRoleAsync(newUser, EntityEnum.SystemRole.User.ToString());
            
            if (!result.Result.Succeeded)
            {
                throw new GlobalException("Failed to create user from Google login", LoginConstant.CreateAccountFailed);
            }
            
            user = newUser;
            
            var info = new UserLoginInfo("Google", claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty, "Google");
            
            var loginResult = await _userManager.AddLoginAsync(newUser, info);
            
            if (!loginResult.Succeeded)
            {
                throw new GlobalException("Failed to add Google login to user", LoginConstant.LoginFailed);
            }
            
            await _signInManager.SignInAsync(user, isPersistent: false);
        }
        else
        {
            var login = await _userManager.GetLoginsAsync(user);
            var googleLogin = login.FirstOrDefault(l => l.LoginProvider == "Google");

            if (googleLogin != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
        }
    }

    public async Task<User> CreateAccount(RegisterRequest request)
    {
        try
        {
            var isUserExist = await _userManager.FindByEmailAsync(request.Email);
            if (isUserExist != null)
            {
                throw new GlobalException("Email already exist", LoginConstant.EmailExists);
            }
            
            var user = new User
            {
                Id = new Guid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                ProfilePictureUrl = request.ProfilePictureUrl,
                CreatedAt = request.CreatedAt
            };
            
            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                throw new GlobalException("Failed to create user", LoginConstant.CreateAccountFailed);
            }
            
            await _userManager.AddToRoleAsync(user, EntityEnum.SystemRole.User.ToString());
            
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            
            string confirmationLink = $"{Environment.GetEnvironmentVariable("BACKEND_URL")}/auth/confirm-email?token={encodedToken}&email={WebUtility.UrlEncode(user.Email)}";
            
            await _emailSender.SendEmailAsync(user.Email, "Verify your email", confirmationLink);
            
            return user;
        }
        catch (Exception ex)
        {
            if (ex is GlobalException)
            {
                throw;
            }
            throw new GlobalException("Failed to create account", LoginConstant.CreateAccountFailed, ex);
        }
    }

    public async Task<User> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new GlobalException("User not found", LoginConstant.AccountNotFound);
        }
        
        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (!isEmailConfirmed)
        {
            throw new GlobalException("Email not confirmed", LoginConstant.EmailNotConfirmed);
        }
        
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        
        if (!result.Succeeded)
        {
            throw new GlobalException("Invalid password", LoginConstant.InvalidPassword);
        }
        
        var (jwtToken, expiry) = _authTokenProcess.GenerateToken(user);
        var refreshToken = _authTokenProcess.GenerateRefreshToken();
        var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
        
        await _userManager.UpdateAsync(user);
        _authTokenProcess.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expiry);
        _authTokenProcess.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshToken, refreshTokenExpiryTime);

        return user;
    }

    public async Task InitiatePasswordReset(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            
            var redisKey = $"{RedisPasswordResetPrefix}{encodedToken}";

            try
            {
                bool result = await _redisDatabase.StringSetAsync(redisKey, user.Id.ToString(), _tokenExpiryTime,
                    When.NotExists);

                if (result)
                {
                    var resetLink =
                        $"{Environment.GetEnvironmentVariable("BACKEND_URL")}/auth/reset-password?token={encodedToken}&email={WebUtility.UrlEncode(user.Email)}";

                    await _emailSender.SendEmailAsync(user.Email!, "Reset your password", resetLink);
                }
                else
                {
                    throw new GlobalException("Password reset token already exists", LoginConstant.ResetPasswordFailed);
                }
            }
            catch (Exception ex)
            {
                throw new GlobalException("Failed to initiate password reset", LoginConstant.ResetPasswordFailed, ex);
            }
        }
        else
        {
            throw new GlobalException("User not found", LoginConstant.AccountNotFound);
        }
    }

    public async Task<bool> VerifyPasswordResetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }
        
        var redisKey = $"{RedisPasswordResetPrefix}{token}";
        try
        {
            return await _redisDatabase.KeyExistsAsync(redisKey);
        }
        catch (Exception ex)
        {
            throw new GlobalException("Failed to verify password reset token", LoginConstant.ResetPasswordFailed, ex);
        }
    }

    public async Task<IdentityResult> ResetPassword(ResetPasswordRequest request)
    {
        var redisKey = $"{RedisPasswordResetPrefix}{request.Token}";
        RedisValue userIdValue;

        try
        {
            userIdValue = await _redisDatabase.StringGetAsync(redisKey);
        }
        catch (Exception ex)
        {
            throw new GlobalException("Failed to retrieve user ID from Redis", LoginConstant.ResetPasswordFailed, ex);
        }
        
        if (!userIdValue.HasValue)
        {
            throw new GlobalException("Invalid or expired password reset token", LoginConstant.ResetPasswordFailed);
        }
        
        var userId = userIdValue.ToString();
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            throw new GlobalException("User not found", LoginConstant.AccountNotFound);
        }

        string decodedToken;
        try
        {
            decodedToken = WebUtility.UrlDecode(request.Token);
        }
        catch (Exception ex)
        {
            throw new GlobalException("Failed to decode token", LoginConstant.ResetPasswordFailed, ex);
        }
        
        IdentityResult result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        
        if (result.Succeeded)
        {
            try
            {
                //* Remove the token from Redis after successful password reset
                await _redisDatabase.KeyDeleteAsync(redisKey);
            }
            catch (Exception ex)
            {
                throw new GlobalException("Failed to update user password", LoginConstant.ResetPasswordFailed, ex);
            }
        }
        else
        {
            throw new GlobalException("Failed to reset password", LoginConstant.ResetPasswordFailed);
        }
        return result;
    }

    public async Task ResendEmailConfirmation(User user)
    {
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            
            string confirmationLink = $"{Environment.GetEnvironmentVariable("BACKEND_URL")}/auth/confirm-email?token={encodedToken}&email={WebUtility.UrlEncode(user.Email)}";
            await _emailSender.SendEmailAsync(user.Email!, "Verify your email", confirmationLink);
        }
        else
        {
            throw new GlobalException("Email already confirmed", LoginConstant.EmailAlreadyConfirmed);
        }
    }

    public async Task RefreshToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new GlobalException("Refresh token cannot be null or empty", "RefreshToken");
        }

        var user = await _userRepository.GetUserByRefreshToken(token);

        if (user == null)
        {
            throw new GlobalException("Invalid refresh token", LoginConstant.RefreshTokenFailed);
        }

        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            throw new GlobalException("Refresh token has expired", LoginConstant.RefreshTokenFailed);
        }
        
        var (jwtToken, expiry) = _authTokenProcess.GenerateToken(user);
        var refreshToken = _authTokenProcess.GenerateRefreshToken();
        var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
        
        await _userManager.UpdateAsync(user);
        _authTokenProcess.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expiry);
        _authTokenProcess.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", refreshToken, refreshTokenExpiryTime);
    }

    public async Task Logout(User user)
    {
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);
        
        _authTokenProcess.DeleteAuthTokenCookie("ACCESS_TOKEN");
        _authTokenProcess.DeleteAuthTokenCookie("REFRESH_TOKEN");
        
        await _signInManager.SignOutAsync();
    }
}