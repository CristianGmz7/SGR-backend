//using Microsoft.AspNetCore.Components;
using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Dtos.AdditionalServices;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionReservasHotelAPI.Controllers;

[Route("api/additionalServices")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class AdditionalServicesController : ControllerBase
{
    private readonly IAdditionalServicesServices _additionalServicesServices;

    public AdditionalServicesController(IAdditionalServicesServices additionalServicesServices)
    {
        this._additionalServicesServices = additionalServicesServices;
    }

    //Crear distintas peticiones similares al RoomsController
    [HttpGet]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}")]
    public async Task<ActionResult<ResponseDto<List<AdditionalServiceDto>>>> GetAll()
    {
        var response = await _additionalServicesServices.GetAdditionalServicesAsync();

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetByHotel/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<AdditionalServiceDto>>> GetAllByHotel(Guid id)
    {
        var response = await _additionalServicesServices.GetAdditionalServicesOneHotelAsync(id);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<AdditionalServiceDto>> GetById(Guid id)
    {
        var response = await _additionalServicesServices.GetAdditionalServiceById(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    [Authorize(Roles = $"{RolesConstant.HOTELADMIN}")]
    public async Task<ActionResult<AdditionalServiceDto>> Create(AdditionalServiceCreateDto dto)
    {
        var response = await _additionalServicesServices.CreateAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{RolesConstant.HOTELADMIN}")]
    public async Task<ActionResult<AdditionalServiceDto>> Edit(AdditionalServiceEditDto dto, Guid id)
    {
        var response = await _additionalServicesServices.EditAsync(dto, id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{RolesConstant.HOTELADMIN}")]
    public async Task<ActionResult<AdditionalServiceDto>> Delete(Guid id)
    {
        var response = await _additionalServicesServices.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }
}
