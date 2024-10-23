using GestionReservasHotelAPI.Dtos.AdditionalServices;
using GestionReservasHotelAPI.Dtos.Rooms;

namespace GestionReservasHotelAPI.Dtos.Reservations;

public class ReservationDto
{
    public Guid Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime FinishDate { get; set; }

    public string Condition { get; set; }

    public double Price { get; set; }

    public string ClientId { get; set; }
    //Posiblemente toque cambiarlo de ClientId a UserId

    //Los List<string> se utilizan en el metodo crear y editar reserva

    public List<RoomDto> RoomsInfoList { get; set; }

    public List<AdditionalServiceDto> AdditionalServicesInfoList { get; set; }
}
