using System.Security.Claims;
using BandCommunity.Application.Common;
using BandCommunity.Domain.DTO.User;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.Interfaces;
using BandCommunity.Shared.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BandCommunity.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("fetch-current-user-data")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser([FromBody] BasicInfoRequest request)
    {
        var response = new BaseResultResponse<User>();
        
        try
        {
            var user = await _userRepository.GetCurrentUserInformation(request);
            if (user == null)
            {
                response.Success = false;
                response.Message = UserConstant.UserNotFound;
                return NotFound(response);
            }

            response.Success = true;
            response.Message = UserConstant.UserFound;
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
}