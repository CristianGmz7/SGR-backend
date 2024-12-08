using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.PayPal;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionReservasHotelAPI.Controllers;

[ApiController]
[Route("api/paypal")]
[Authorize(AuthenticationSchemes = "Bearer")]

public class PayPalController : ControllerBase
{
    private readonly IPayPalService _payPalService;

    public PayPalController(IPayPalService payPalService)
    {
        this._payPalService = payPalService;
    }

    [HttpPost("refund")]
    [Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    public async Task<ActionResult<ResponseDto<OrderResponseDto>>> Refund(RefundOrderDto dto)
    {
        var response = await _payPalService.RefundPaymentAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    //[HttpPost("create-order")]
    //[Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    //public async Task<ActionResult<ResponseDto<OrderResponseDto>>> CreateOrder (CreateOrderDto dto)
    //{
    //    var response = await _payPalService.CreateOrderAsync(dto);
    //    return StatusCode(response.StatusCode, response);
    //}

    //[HttpPost("capture-order/{orderId}")]
    //[Authorize(Roles = $"{RolesConstant.PAGEADMIN}, {RolesConstant.HOTELADMIN}, {RolesConstant.USER}")]
    //public async Task<ActionResult<ResponseDto<OrderResponseDto>>> CaptureOrder(string orderId)
    //{
    //    var response = await _payPalService.CaptureOrderAsync(orderId);
    //    return StatusCode(response.StatusCode, response);
    //}

}
