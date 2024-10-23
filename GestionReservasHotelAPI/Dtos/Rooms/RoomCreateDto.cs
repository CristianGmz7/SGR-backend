using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace GestionReservasHotelAPI.Dtos.Rooms;

public class RoomCreateDto
{
    //v. rep.
    [Display(Name = "Numero de habitacion")]
    [Range(1, int.MaxValue, ErrorMessage = "El {0} debe ser mayor o igual a 1")]
    [Required(ErrorMessage = "El {0} es obligatorio")]
    public int NumberRoom { get; set; }

    [Display(Name = "Tipo de habitacion")]
    [RegularExpression("^(SENCILLA|DOBLE|SUITE)$", ErrorMessage = "El {0} solo puede ser 'SENCILLA', 'DOBLE', 'SUITE'")]
    [Required(ErrorMessage = "El {0} es obligatorio")]
    public string TypeRoom { get; set; }

    [Display(Name = "Precio por noche")]
    [Range(1, double.MaxValue, ErrorMessage = "El {0} debe ser mayor o igual a 1")]
    [Required(ErrorMessage = "El {0} es obligatorio")]
    public double PriceNight { get; set; }

    [Display(Name = "url de la imagen")]
    [Required(ErrorMessage = "La {0} es obligatoria")]
    public string ImageUrl { get; set; }

    public Guid HotelId { get; set; }

}
