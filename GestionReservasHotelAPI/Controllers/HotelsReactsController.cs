using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Reactions;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GestionReservasHotelAPI.Controllers;

[Route("api/hotelsReacts")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
//esto solo lo pueden usarlo los que estan logueados
public class HotelsReactsController : ControllerBase
{
    private readonly IHotelsReactsService _hotelsReactsService;

    public HotelsReactsController(IHotelsReactsService hotelsReactsService)
    {
        this._hotelsReactsService = hotelsReactsService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ResponseDto<HotelReactResponseDto>>> GetByHotelAndUser(Guid HotelId, string isAuthenticated)
    {
        var response = await _hotelsReactsService.GetByHotelAndUserAsync(HotelId, isAuthenticated);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<HotelReactResponseDto>>> Create(HotelReactCreateDto dto)
    {
        var response = await _hotelsReactsService.CreateAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<HotelReactResponseDto>>> Edit (HotelReactEditDto dto)
    {
        var response = await _hotelsReactsService.EditAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<HotelReactResponseDto>>> Delete (Guid HotelId, string Action)
    {
        var response = await _hotelsReactsService.DeleteAsync(HotelId, Action);
        return StatusCode(response.StatusCode, response);
    }
}
