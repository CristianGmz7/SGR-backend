using GestionReservasHotelAPI.Dtos.Auth;
using GestionReservasHotelAPI.Dtos.Common;
using System.Security.Claims;

namespace GestionReservasHotelAPI.Services.Interfaces
{
    public interface IAuthService
    {
        ClaimsPrincipal GetTokenPrincipal(string token);

        //string GetUserId();
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ResponseDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
        Task<ResponseDto<LoginResponseDto>> RegisterAsync(RegisterDto dto);
    }
}
