namespace GestionReservasHotelAPI.Dtos.AdditionalServices;

public class AdditionalServiceDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public double Price { get; set; }

    public Guid HotelId { get; set; }
}
