using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.Reservations;

public class ReservationEditDto
{
    [Display(Name = "Fecha inicial")]
    [Required(ErrorMessage = "La {0} es obligatoria")]
    public DateTime StartDate { get; set; }

    [Display(Name = "Fecha final")]
    [Required(ErrorMessage = "La {0} es obligatoria")]
    public DateTime FinishDate { get; set; }

    //el id del cliente no se necesita

    [Display(Name = "Lista de habitaciones")]
    [Required(ErrorMessage = "La {0} debe tener al menos un elemento")]
    public List<string> RoomsList { get; set; }

    [Display(Name = "Lista de habitaciones")]
    public List<string> AdditionalServicesList { get; set; }
}
