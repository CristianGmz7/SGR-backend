using GestionReservasHotelAPI.Dtos.Hotels;

namespace GestionReservasHotelAPI.Dtos.Rooms;

public class RoomDto
{
    public Guid Id { get; set; }        

    public int NumberRoom { get; set; }

    public string TypeRoom { get; set; }

    public double PriceNight { get; set; }

    //public Guid HotelId { get; set; } ya no se necesita creo, porque viene el Dto

    public string ImageUrl { get; set; }

    public HotelDto HotelInfo { get; set; }

    //Esta se utilizará cuando los administradores hagan peticion obtener todas habitaciones
    public string Condition { get; set; }

    //(no necesito ver por el momento las reservaciones que tenga)

}
