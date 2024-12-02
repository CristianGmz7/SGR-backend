using GestionReservasHotelAPI.Dtos.AdditionalServices;
using GestionReservasHotelAPI.Dtos.Rooms;
using GestionReservasHotelAPI.Dtos.Users;

namespace GestionReservasHotelAPI.Dtos.Reservations;

public class ReservationAdminHotelResponseDto
{
    public Guid Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime FinishDate { get; set; }

    public string Condition { get; set; }

    public double Price { get; set; }
    public BasicUserInformationResponseDto Client { get; set; }

    public List<RoomDto> RoomsInfoList { get; set; }

    public List<AdditionalServiceDto> AdditionalServicesInfoList { get; set; }
}
