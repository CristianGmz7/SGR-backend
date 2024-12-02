using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionReservasHotelAPI.Database.Entities;

[Table("hotels", Schema = "dbo")]
public class HotelEntity : BaseEntity
{
    [StringLength(50)]
    [Column("name")]
    public string Name { get; set; }

    [StringLength(100)]
    [Column("address")]
    public string Address { get; set; }

    [Range(1, 5)]
    [Column("stars_michelin")]
    public int StarsMichelin { get; set; }

    [RegularExpression("^[0-9]{8}$")]       
    [Column("number_phone")]
    public int NumberPhone { get; set; }

    [StringLength(100)]
    [Column("overview")]
    public string Overview { get; set; }

    [StringLength(500)]
    [Column("description")]
    public string Description { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; }

    // agregado campos del propietario / administrador del hotel
    [Column("admin_user_id")]
    public string AdminUserId { get; set; }

    [ForeignKey(nameof(AdminUserId))]
    public UserEntity AdminUserEntity { get; set; }

    //llaves foraneas auditoria
    public virtual UserEntity CreatedByUser { get; set; }

    public virtual UserEntity UpdatedByUser { get; set; }
}
