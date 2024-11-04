using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.Auth;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "El Token es requerido")]
    public string Token { get; set; }

    [Required(ErrorMessage = "El RefreshToken es requerido")]
    public string RefreshToken { get; set; }
}
