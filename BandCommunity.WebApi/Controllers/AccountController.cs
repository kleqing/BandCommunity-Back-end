using System.Security.Claims;
using BandCommunity.Application.Common;
using BandCommunity.Domain.DTO.Auth;
using BandCommunity.Domain.Entities;
using BandCommunity.Infrastructure.Auth;
using BandCommunity.Shared.Constant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BandCommunity.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountServices _accountServices;
    private readonly UserManager<User> _userManager;

    private readonly string _frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? string.Empty;

    public AccountController(IAccountServices accountServices, UserManager<User> userManager)
    {
        _accountServices = accountServices;
        _userManager = userManager;
    }

    [HttpGet("login/google")]
    [AllowAnonymous]
    public IActionResult LoginGoogle(string returnUrl)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("login/google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl)
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return BadRequest("External authentication failed.");
        }

        var claimsPrincipal = result.Principal;
        await _accountServices.LoginWithGoogle(claimsPrincipal);

        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        var name = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value + " " +
                   claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value;
        var avatar = claimsPrincipal.FindFirst("picture")?.Value;

        var returnToFrontend =
            $"{_frontendUrl}?email={Uri.EscapeDataString(email!)}&name={Uri.EscapeDataString(name)}&avatar={Uri.EscapeDataString(avatar!)}";
        return Redirect(returnToFrontend);
    }

    [HttpPost("create-account")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAccount([FromBody] RegisterRequest request)
    {
        var response = new BaseResultResponse<User>();

        try
        {
            var user = await _accountServices.CreateAccount(request);

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
            var user = await _accountServices.Login(request);

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
            await _accountServices.InitiatePasswordReset(request.Email);
            response.Success = true;
            response.Message = LoginConstant.SendSuccess;
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

        var isValid = await _accountServices.VerifyPasswordResetToken(token);
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
            IdentityResult result = await _accountServices.ResetPassword(request);

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

            await _accountServices.ResendEmailConfirmation(user);
            response.Success = true;
            response.Message = LoginConstant.SendSuccess;
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
        await _accountServices.RefreshToken(refreshToken);
        return Ok();
    }
    
    [Authorize(AuthenticationSchemes = "MyCookie")]
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
            await _accountServices.Logout(user);

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