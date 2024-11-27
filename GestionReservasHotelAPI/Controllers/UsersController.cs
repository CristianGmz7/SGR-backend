using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Users;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GestionReservasHotelAPI.Controllers;

[Route("api/users")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        this._usersService = usersService;
    }

    [HttpGet("allUsersWithUserRole")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}")]
    public async Task<ActionResult<ResponseDto<List<BasicUserInformationResponseDto>>>> GetUsersWithUserRole(string searchTerm)
    {
        var response = await _usersService.GetUsersListWithUserRoleAsync(searchTerm);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("allUsers")]
    [Authorize(Roles = $"{RolesConstant.HOTELADMIN}")]
    public async Task<ActionResult<ResponseDto<List<BasicUserInformationResponseDto>>>> GetAllUsers()
    {
        var response = await _usersService.GetUserListAsync();
        return StatusCode(response.StatusCode, response);
    }
}
