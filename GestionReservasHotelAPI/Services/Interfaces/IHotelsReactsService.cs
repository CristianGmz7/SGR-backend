using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Reactions;

namespace GestionReservasHotelAPI.Services.Interfaces;

public interface IHotelsReactsService
{
    Task<ResponseDto<HotelReactResponseDto>> CreateAsync(HotelReactCreateDto dto);
    Task<ResponseDto<HotelReactResponseDto>> DeleteAsync(Guid HotelId, string Action);
    Task<ResponseDto<HotelReactResponseDto>> EditAsync(HotelReactEditDto dto);
    Task<ResponseDto<HotelReactResponseDto>> GetByHotelAndUserAsync(Guid HotelId, string isAuthenticated);
}
