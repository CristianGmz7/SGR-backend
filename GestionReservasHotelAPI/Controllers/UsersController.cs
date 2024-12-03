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
    public async Task<ActionResult<ResponseDto<List<BasicUserInformationResponseDto>>>> GetAllUsers(string searchTerm)
    {
        var response = await _usersService.GetUserListAsync(searchTerm);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("userLogged")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<UserLoggedResponseDto>>> GetUserLogged()
    {
        var response = await _usersService.GetUserInfoLoggedAsync();
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("editUserInfo")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<BasicUserInformationResponseDto>>> EditUserInfo (UserEditDto dto)
    {
        var response = await _usersService.EditUserAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("confirmPasswordUserToEdit")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<BasicUserInformationResponseDto>>> ConfirmPasswordUserToEdit (string password)
    {
        var response = await _usersService.ConfirmPasswordUserToEditAsync(password);
        return StatusCode(response.StatusCode, response);
    }
}
