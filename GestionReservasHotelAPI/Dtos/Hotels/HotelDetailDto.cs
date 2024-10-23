using GestionReservasHotelAPI.Dtos.Rooms;

namespace GestionReservasHotelAPI.Dtos.Hotels;
 
public class HotelDetailDto
{
    public HotelDto Hotel { get; set; }
    public List<RoomDto> Rooms { get; set; }
}