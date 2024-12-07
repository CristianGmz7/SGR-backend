namespace GestionReservasHotelAPI.Dtos.Reactions;

public class HotelReactResponseDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string UserId { get; set; }
    public bool Reaction { get; set; }
    public string Action { get; set; }
}
