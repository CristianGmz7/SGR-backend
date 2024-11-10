using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Reservations;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

//using Microsoft.AspNetCore.Components;        //COMENTAR / ELIMINAR ESTA LIBRERIA PARA EVITAR ERROR EN LA RUTA
using Microsoft.AspNetCore.Mvc;

namespace GestionReservasHotelAPI.Controllers;

[Route("api/reservations")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]

public class ReservationsController : ControllerBase
{
    private readonly IReservationsService _reservationsService;

    public ReservationsController(IReservationsService reservationsService)
    {
        this._reservationsService = reservationsService;
    }

    [HttpGet]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<PaginationDto<List<ReservationDto>>>>> PaginationList(
        string clientId = "", int page = 1)
    {
        var response = await _reservationsService.GetReservationListAsync(clientId, page);

        return StatusCode(response.StatusCode, new
        {
            response.Status,
            response.Message,
            response.Data
        });
    }

    [HttpGet("GetBetweenDates")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<PaginationDto<List<ReservationDto>>>>> PaginationListBetweenDates(
        string clientId = "", int page = 1, 
        DateTime filterStartDate = default, DateTime filterEndDate = default)
    {
        var response = await _reservationsService.GetReservationListBetweenDates(
            clientId, page, filterStartDate, filterEndDate);

        return StatusCode(response.StatusCode, new
        {
            response.Status,
            response.Message,
            response.Data
        });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<ReservationDto>>> GetOneById (Guid id)
    {
        var response = await _reservationsService.GetReservationByIdAsync(id);
        return StatusCode(response.StatusCode, new
        {
            response.Status,
            response.Message,
            response.Data
        });
    }

    [HttpPost]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<ReservationDto>>> Create(ReservationCreateDto dto)
    {
        var response = await _reservationsService.CreateReservationAsync(dto);

        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<ReservationDto>>> Edit(ReservationEditDto dto, Guid id)
    {
        var response = await _reservationsService.EditReservationAsync(dto, id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<ReservationDto>>> Delete (Guid id)
    {
        var response = await _reservationsService.DeleteReservationAsync(id);

        return StatusCode(response.StatusCode, response);
    }
}
