using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestionReservasHotelAPI.Database.Entities;

public class UserEntity : IdentityUser
{
    [StringLength(70, MinimumLength = 3)]
    [Column("first_name")]
    [Required]
    public string FirstName { get; set; }

    [StringLength(70, MinimumLength = 3)]
    [Column("last_name")]
    [Required]
    public string LastName { get; set; }

    [StringLength(450)]
    [Column("refresh_token")]
    public string RefreshToken { get; set; }

    [Column("refresh_token_expire")]
    public DateTime RefreshTokenExpire { get; set; }

    //agregado para funcionalidad del seeder: la de que haya un usuario por hotel
    //Comentar despues de ejecutar el seeder y descomentar sobreescritura SaveChangesAsync Context
    // Se debe comentar también el metodo LoadHotelAsync del Seeder despues de ejecutarse
    public ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();

}
