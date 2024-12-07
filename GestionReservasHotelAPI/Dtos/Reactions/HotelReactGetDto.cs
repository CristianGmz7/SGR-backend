using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.Reactions;

public class HotelReactGetDto
{
    [Display(Name = "Hotel Id")]
    [Required(ErrorMessage = "El {0} es requerido.")]
    public Guid HotelId { get; set; }

    //Adaptar si es necesario
    //[Display(Name = "Accion")]
    //[RegularExpression("^(SWITCHLIKED)$", ErrorMessage = "La {0} solo puede ser 'SWITCHLIKED'")]
    //[Required(ErrorMessage = "La {0} es requerida.")]
    //public string Action { get; set; }

    [Display(Name = "Autenticación")]
    [Required(ErrorMessage = "La {0} es requerida.")]
    public bool IsAuthenticated { get; set; }
}
