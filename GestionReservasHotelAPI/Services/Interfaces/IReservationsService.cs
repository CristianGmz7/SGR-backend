using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Reservations;

namespace GestionReservasHotelAPI.Services.Interfaces;

public interface IReservationsService
{
    Task<ResponseDto<ReservationDto>> CreateReservationAsync(ReservationCreateDto dto);
    Task<ResponseDto<ReservationDto>> DeleteReservationAsync(Guid id);
    Task<ResponseDto<ReservationDto>> EditReservationAsync(ReservationEditDto dto, Guid id);
    Task<ResponseDto<ReservationDto>> GetReservationByIdAsync(Guid id);
    Task<ResponseDto<PaginationDto<List<ReservationDto>>>> GetReservationListAsync(string clientId = "", int page = 1);
    Task<ResponseDto<PaginationDto<List<ReservationDto>>>> GetReservationListBetweenDates(string clientId = "", int page = 1, DateTime filterStartDate = default, DateTime filterEndDate = default);
}
