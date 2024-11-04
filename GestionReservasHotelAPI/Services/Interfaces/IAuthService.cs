using GestionReservasHotelAPI.Dtos.Auth;
using GestionReservasHotelAPI.Dtos.Common;

namespace GestionReservasHotelAPI.Services.Interfaces
{
    public interface IAuthService
    {
        //string GetUserId();
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ResponseDto<LoginResponseDto>> RegisterAsync(RegisterDto dto);
    }
}
