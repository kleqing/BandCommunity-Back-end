using System.Security.Claims;
using BandCommunity.Application.Common;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.Models.Auth;
using BandCommunity.Infrastructure.Auth;
using BandCommunity.Shared.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BandCommunity.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizeController : ControllerBase
{
    private readonly IAuthorizeService _authorizeService;
    private readonly UserManager<User> _userManager;

    private readonly string _frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? string.Empty;

    public AuthorizeController(IAuthorizeService authorizeService, UserManager<User> userManager)
    {
        _authorizeService = authorizeService;
        _userManager = userManager;
    }

    [HttpPost("create-account")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAccount([FromBody] RegisterRequest request)
    {
        var response = new BaseResultResponse<User>();

        try
        {
            var user = await _authorizeService.CreateAccount(request);

            response.Success = true;
            response.Message = LoginConstant.AccountCreated;
            response.Data = user;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            return BadRequest(response);
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = new BaseResultResponse<User>();

        try
        {
            var user = await _authorizeService.Login(request);

            response.Success = true;
            response.Message = LoginConstant.LoginSuccess;
            response.Data = user;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            return BadRequest(response);
        }
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return BadRequest(LoginConstant.AccountNotFound);
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return Redirect($"{_frontendUrl}/verify-success?verifiedEmail={Uri.EscapeDataString(user.Email!)}");
        }

        return BadRequest(LoginConstant.EmailNotConfirmed);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var response = new BaseResultResponse<string>();

        try
        {
            await _authorizeService.InitiatePasswordReset(request.Email);
            response.Success = true;
            response.Message = LoginConstant.SendEmailSuccess;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            return BadRequest(response);
        }
    }

    [HttpGet("reset-password/verify")]
    public async Task<IActionResult> VerifyResetToken([FromQuery] string token)
    {
        var response = new BaseResultResponse<bool>();

        if (string.IsNullOrEmpty(token))
        {
            response.Success = false;
            response.Message = "Reset token is required.";
            return BadRequest(response);
        }

        var isValid = await _authorizeService.VerifyPasswordResetToken(token);
        if (!isValid)
        {
            response.Success = false;
            response.Message = LoginConstant.InvalidToken;
            return BadRequest(response);
        }

        response.Success = true;
        response.Message = LoginConstant.ValidToken;
        return Ok(response);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var response = new BaseResultResponse<string>();
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest("Reset token is required.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest("New password and confirm password do not match.");
        }

        try
        {
            IdentityResult result = await _authorizeService.ResetPassword(request);

            if (result.Succeeded)
            {
                response.Success = true;
                response.Message = LoginConstant.PasswordResetSuccess;
                return Ok(response);
            }
            else
            {
                response.Success = false;
                response.Message = LoginConstant.ResetPasswordFailed;
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            return BadRequest(response);
        }
    }

    [HttpPost("resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailRequest email)
    {
        var response = new BaseResultResponse<User>();
        try
        {
            var user = await _userManager.FindByEmailAsync(email.Email);
            if (user == null)
            {
                response.Success = false;
                response.Message = LoginConstant.AccountNotFound;
                return BadRequest(response);
            }

            await _authorizeService.ResendEmailConfirmation(user);
            response.Success = true;
            response.Message = LoginConstant.SendEmailSuccess;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            return BadRequest(response);
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = HttpContext.Request.Cookies["REFRESH_TOKEN"];
        await _authorizeService.RefreshToken(refreshToken);
        return Ok();
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var response = new BaseResultResponse<string>();

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                response.Success = false;
                response.Message = "User not found.";
                return BadRequest(response);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Success = false;
                response.Message = LoginConstant.AccountNotFound;
                return BadRequest(response);
            }

            // Sign out the user
            await _authorizeService.Logout(user);

            response.Success = true;
            response.Message = LoginConstant.LogoutSuccess;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            return BadRequest(response);
        }
    }
}