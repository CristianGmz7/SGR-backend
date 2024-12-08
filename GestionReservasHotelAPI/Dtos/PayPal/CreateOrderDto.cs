namespace GestionReservasHotelAPI.Dtos.PayPal;

//no se usará
public class CreateOrderDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}
