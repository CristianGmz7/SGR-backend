//using Microsoft.AspNetCore.Components;
using GestionReservasHotelAPI.Dtos.AdditionalServices;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionReservasHotelAPI.Controllers;

[Route("api/additionalServices")]
[ApiController]

public class AdditionalServicesController : ControllerBase
{
    private readonly IAdditionalServicesServices _additionalServicesServices;

    public AdditionalServicesController(IAdditionalServicesServices additionalServicesServices)
    {
        this._additionalServicesServices = additionalServicesServices;
    }

    //Crear distintas peticiones similares al RoomsController
    [HttpGet]
    public async Task<ActionResult<ResponseDto<List<AdditionalServiceDto>>>> GetAll()
    {
        var response = await _additionalServicesServices.GetAdditionalServicesAsync();

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("GetByHotel/{id}")]
    public async Task<ActionResult<List<AdditionalServiceDto>>> GetAllByHotel(Guid id)
    {
        var response = await _additionalServicesServices.GetAdditionalServicesOneHotelAsync(id);

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdditionalServiceDto>> GetById(Guid id)
    {
        var response = await _additionalServicesServices.GetAdditionalServiceById(id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<AdditionalServiceDto>> Create(AdditionalServiceCreateDto dto)
    {
        var response = await _additionalServicesServices.CreateAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdditionalServiceDto>> Edit(AdditionalServiceEditDto dto, Guid id)
    {
        var response = await _additionalServicesServices.EditAsync(dto, id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<AdditionalServiceDto>> Delete(Guid id)
    {
        var response = await _additionalServicesServices.DeleteAsync(id);
        return StatusCode(response.StatusCode, response);
    }
}
