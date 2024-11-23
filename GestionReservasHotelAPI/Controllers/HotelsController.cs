using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Hotels;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionReservasHotelAPI.Controllers;

[ApiController]
[Route("api/hotels")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class HotelsController : ControllerBase
{
    private readonly IHotelsService _hotelsService;

    public HotelsController(IHotelsService hotelsService)
    {
        this._hotelsService = hotelsService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ResponseDto<PaginationDto<List<HotelDto>>>>> GetAll(int page = 1)
    {
        var response = await _hotelsService.GetHotelsListAsync(page);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ResponseDto<List<HotelDto>>>> Get(Guid id)
    {
        var response = await _hotelsService.GetHotelByIdAsync(id);

        return StatusCode(response.StatusCode, response);
    }

    //al ejecutarse este endpoint posteriormente se debe actualizar el rol del usuario que se acaba de añadirlo como su administrador
    [HttpPost]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}")]
    public async Task<ActionResult<ResponseDto<List<HotelDto>>>> Create(HotelCreateDto dto)
    {
        var response = await _hotelsService.CreateAsync(dto);

        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{RolesConstant.HOTELADMIN}")]
    public async Task<ActionResult<ResponseDto<List<HotelDto>>>> Edit(HotelEditDto dto, Guid id)
    {
        var response = await _hotelsService.EditAsync(dto, id);

        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}")]
    public async Task<ActionResult<ResponseDto<List<HotelDto>>>> Delete(Guid id)
    {
        var response = await _hotelsService.DeleteAsync(id);

        return StatusCode(response.StatusCode, response);
    }
}
