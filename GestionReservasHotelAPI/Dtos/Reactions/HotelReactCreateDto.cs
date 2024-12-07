using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.Reactions;

public class HotelReactCreateDto
{
    [Display(Name = "Hotel Id")]
    [Required(ErrorMessage = "El {0} es requerido.")]
    public Guid HotelId { get; set; }

    [Display(Name = "Accion")]
    [RegularExpression("^(LIKED|UNLIKED)$", ErrorMessage = "La {0} solo puede ser 'LIKED', 'UNLIKED'")]
    [Required(ErrorMessage = "La {0} es requerida.")]
    public string Action { get; set; }
    //public string UserId { get; set; }        //se obtiene del backend
}
