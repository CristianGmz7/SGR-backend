using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Users;

namespace GestionReservasHotelAPI.Services.Interfaces;

public interface IUsersService
{
    Task<ResponseDto<UserResponseDto>> ChangeRoleAsync(UserChangeRoleDto dto, string id);
}
