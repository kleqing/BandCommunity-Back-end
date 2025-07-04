using System.Net;
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

public class AuthorizeService : IAuthorizeService
{
    private readonly IAuthTokenProcess _authTokenProcess;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IDatabase _redisDatabase;
    private readonly TimeSpan _tokenExpiryTime;
    private const string RedisPasswordResetPrefix = "reset-password:";

    public AuthorizeService(
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
        _tokenExpiryTime =
            TimeSpan.FromMinutes(int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "15"));
    }

    public async Task<User> CreateAccount(RegisterRequest request)
    {
        try
        {
            var isUserExist = await _userManager.FindByEmailAsync(request.Email);
            if (isUserExist != null)
            {
                throw new GlobalException("Register", LoginConstant.EmailExists);
            }

            var checkUserNameExist = await _userManager.FindByNameAsync(request.Username);
            if (checkUserNameExist != null)
            {
                throw new GlobalException("Register", LoginConstant.UsernameExists);
            }

            var user = new User
            {
                Id = new Guid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                ProfilePictureUrl = request.ProfilePictureUrl,
                CreatedAt = request.CreatedAt
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new GlobalException("Register", LoginConstant.CreateAccountFailed);
            }

            await _userManager.AddToRoleAsync(user, EntityEnum.SystemRole.User.ToString());

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            string confirmationLink =
                $"{Environment.GetEnvironmentVariable("BACKEND_URL")}/api/Authorize/confirm-email?userId={user.Id}&token={encodedToken}";

            await _emailSender.SendEmailAsync(user.Email, "Verify your email", confirmationLink);

            return user;
        }
        catch (GlobalException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create account", ex);
        }
    }

    public async Task<User> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new GlobalException("Login", LoginConstant.AccountNotFound);
        }

        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (!isEmailConfirmed)
        {
            throw new GlobalException("Login", LoginConstant.EmailNotConfirmed);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            throw new GlobalException("Login", LoginConstant.InvalidPassword);
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
            var redisKey = $"{RedisPasswordResetPrefix}{token}";

            try
            {
                bool result = await _redisDatabase.StringSetAsync(redisKey, user.Id.ToString(), _tokenExpiryTime,
                    When.NotExists);

                if (result)
                {
                    var encodedToken = Uri.EscapeDataString(token);
                    var resetLink =
                        $"{Environment.GetEnvironmentVariable("FRONTEND_URL")}/reset-password?token={encodedToken}";

                    await _emailSender.SendEmailAsync(user.Email!, "Reset your password", resetLink);
                }
                else
                {
                    throw new GlobalException("Reset", LoginConstant.PasswordResetTokenExists);
                }
            }
            catch (Exception ex)
            {
                throw new GlobalException("Initiate", LoginConstant.PasswordResetFailed, ex);
            }
        }
        else
        {
            throw new GlobalException("Find user in database", LoginConstant.AccountNotFound);
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
            throw new GlobalException("Verify", LoginConstant.PasswordResetTokenInvalid, ex);
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
            throw new GlobalException("Retrieve user", LoginConstant.RedisUserIdNotFound, ex);
        }

        if (!userIdValue.HasValue)
        {
            throw new GlobalException("Validate token", LoginConstant.InvalidPasswordResetToken);
        }

        var userId = userIdValue.ToString();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new GlobalException("Find user in database", LoginConstant.AccountNotFound);
        }

        IdentityResult result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (result.Succeeded)
        {
            try
            {
                //* Remove the token from Redis after successful password reset
                await _redisDatabase.KeyDeleteAsync(redisKey);
            }
            catch (Exception ex)
            {
                throw new GlobalException("Update", LoginConstant.UpdatePasswordFailed, ex);
            }
        }
        else
        {
            throw new GlobalException("Reset", LoginConstant.ResetPasswordFailed);
        }

        return result;
    }

    public async Task ResendEmailConfirmation(User user)
    {
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            string confirmationLink =
                $"{Environment.GetEnvironmentVariable("BACKEND_URL")}/api/Authorize/confirm-email?userId={user.Id}&token={encodedToken}";
            await _emailSender.SendEmailAsync(user.Email!, "Verify your email", confirmationLink);
        }
        else
        {
            throw new GlobalException("Verify", LoginConstant.EmailAlreadyConfirmed);
        }
    }

    public async Task RefreshToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new GlobalException("Check token", "Refresh token cannot be null or empty");
        }

        var user = await _userRepository.GetUserByRefreshToken(token);

        if (user == null)
        {
            throw new GlobalException("Check token", LoginConstant.InvalidRefreshToken);
        }

        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            throw new GlobalException("Check token", LoginConstant.RefreshTokenExpired);
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
    }
}