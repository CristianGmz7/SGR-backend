using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.AdditionalServices;

public class AdditionalServiceEditDto
{
    [Display(Name = "Nombre")]
    [StringLength(100, ErrorMessage = "El {0} debe tener menos de {1} caracteres")]
    [Required(ErrorMessage = "El {0} es obligatorio")]
    public string Name { get; set; }

    [Display(Name = "Precio")]
    [Range(1, double.MaxValue, ErrorMessage = "El {0} debe ser mayor o igual a 1")]
    [Required(ErrorMessage = "El {0} es obligatorio")]
    public double Price { get; set; }
}
