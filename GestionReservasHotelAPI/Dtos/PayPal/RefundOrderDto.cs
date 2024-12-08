namespace GestionReservasHotelAPI.Dtos.PayPal;

public class RefundOrderDto
{
    public string CaptureId { get; set; } // ID de la captura de PayPal
    public string Reason { get; set; }    // (Opcional) Razón del reembolso
}
