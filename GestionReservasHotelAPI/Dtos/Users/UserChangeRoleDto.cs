using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.Users;

public class UserChangeRoleDto
{
    [Required]
    public string RolId { get; set; }
}
