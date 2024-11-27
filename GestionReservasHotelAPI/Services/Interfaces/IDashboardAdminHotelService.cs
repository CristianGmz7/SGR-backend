using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Dashboards.DashboardAdminHotel;

namespace GestionReservasHotelAPI.Services.Interfaces;

public interface IDashboardAdminHotelService
{
    Task<ResponseDto<DashboardHotelIdResponseDto>> GetHotelIdAsync();
}
