using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Users;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GestionReservasHotelAPI.Controllers;

[Route("api/dashboardAdminHotel")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class DashboardAdminHotelController : ControllerBase
{
    private readonly IDashboardAdminHotelService _dashboardAdminHotelService;

    public DashboardAdminHotelController(IDashboardAdminHotelService dashboardAdminHotelService)
    {
        this._dashboardAdminHotelService = dashboardAdminHotelService;
    }

    [HttpGet("getHotelId")]
    [Authorize(Roles = $"{RolesConstant.HOTELADMIN}")]
    public async Task<ActionResult<ResponseDto<BasicUserInformationResponseDto>>> GetHotelId()
    {
        var response = await _dashboardAdminHotelService.GetHotelIdAsync();
        return StatusCode(response.StatusCode, response);
    }
}
