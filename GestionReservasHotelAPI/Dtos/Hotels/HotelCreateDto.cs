using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.Hotels
{
    public class HotelCreateDto
    {
        [Display(Name = "Nombre")]
        [Required(ErrorMessage ="El {0} del hotel es requerido.")]
        public string Name { get; set; }

        [Display(Name = "Direccion")]
        [StringLength(100)]
        [Required]
        public string Address { get; set; }

        [Display(Name = "Estrellas Michelin")]
        [Range(1, 5, ErrorMessage = "La {0} debe ser un número entre {1} y {5}.")]
        [Required]
        public int StarsMichelin { get; set; }

        [Display(Name = "Numero de Telefono")]
        [RegularExpression("^[0-9]{8}$", ErrorMessage = "El {0} debe tener exactamente 8 dígitos.")]
        [Required]
        public int NumberPhone { get; set; }

        [Display(Name = "Overview")]
        [StringLength(100)]
        [Required]
        public string Overview { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(500)]
        [Required]
        public string Description { get; set; }

        [Display(Name = "url de la imagen")]
        [Required]
        public string ImageUrl { get; set; }
    }
}
