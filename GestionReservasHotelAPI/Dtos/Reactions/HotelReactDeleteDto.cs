using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.Reactions;

public class HotelReactDeleteDto
{
    [Display(Name = "Hotel Id")]
    [Required(ErrorMessage = "El {0} es requerido.")]
    public Guid HotelId { get; set; }

    [Display(Name = "Accion")]
    [RegularExpression("^(REMOVELIKED|REMOVEUNLIKED)$", ErrorMessage = "La {0} solo puede ser 'REMOVELIKED', 'REMOVEUNLIKED'")]
    [Required(ErrorMessage = "La {0} es requerida.")]
    public string Action { get; set; }
}
