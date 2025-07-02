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

    [HttpPost("update-basic-info")]
    [Authorize]
    public async Task<IActionResult> UpdateBasicUserInformation([FromBody] BasicInfoRequest request)
    {
        var response = new BaseResultResponse<User>();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out var userId))
        {
            response.Success = false;
            response.Message = UserConstant.InvalidUserId;
            return Unauthorized(response);
        }

        var result = await _userRepository.UpdateBasicUserInformation(request, userId);

        if (result == null)
        {
            response.Success = false;
            response.Message = UserConstant.UserNotFound;
            return NotFound(response);
        }

        response.Success = true;
        response.Message = UserConstant.UserInformationUpdated;
        response.Data = result;
        return Ok(response);
    }
}