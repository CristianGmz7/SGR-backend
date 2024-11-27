using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Users;

namespace GestionReservasHotelAPI.Services.Interfaces;

public interface IUsersService
{
    Task<ResponseDto<List<BasicUserInformationResponseDto>>> GetUserListAsync();
    Task<ResponseDto<List<BasicUserInformationResponseDto>>> GetUsersListWithUserRoleAsync(string searchTerm = "");
}
