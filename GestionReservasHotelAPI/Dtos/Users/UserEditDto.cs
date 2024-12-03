using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Dtos.Users;

public class UserEditDto
{
    [Display(Name = "nombres")]
    public string FirstName { get; set; }

    [Display(Name = "apellidos")]
    public string LastName { get; set; }

    [Display(Name = "foto de perfil")]
    public string ProfilePictureUrl { get; set; }

    [Display(Name = "Correo Electronico Nuevo")]
    //[EmailAddress(ErrorMessage = "EL campo {0} no es valido")]
    public string NewEmail { get; set; }

    [Display(Name = "Correo Electronico Actual")]
    //[EmailAddress(ErrorMessage = "EL campo {0} no es valido")]
    public string OldEmail { get; set; }

    [Display(Name = "Contraseña Nueva")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "La contraseña debe ser segura y contener al menos 8 caracteres, incluyendo minúsculas, mayúsculas, números y caracteres especiales.")]
    public string NewPassword { get; set; }

    //[Display(Name = "Contraseña Actual")]
    //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "La contraseña debe ser segura y contener al menos 8 caracteres, incluyendo minúsculas, mayúsculas, números y caracteres especiales.")]
    public string OldPassword { get; set; }
}
