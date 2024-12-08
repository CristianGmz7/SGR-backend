using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.PayPal;

namespace GestionReservasHotelAPI.Services.Interfaces;

public interface IPayPalService
{
    Task<ResponseDto<string>> CaptureOrderAsync(string orderId);
    Task<ResponseDto<OrderResponseDto>> CreateOrderAsync(CreateOrderDto dto);
    Task<ResponseDto<string>> RefundPaymentAsync(RefundOrderDto dto);
}
