using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionReservasHotelAPI.Database.Entities;

[Table("reservations", Schema = "dbo")]
public class ReservationEntity : BaseEntity
{
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("finish_date")]
    public DateTime FinishDate { get; set; }

    [Range(1, double.MaxValue)]
    [Column("price")]
    public double Price { get; set; }

    [Column("client_id")]
    public string ClientId { get; set; }

    [ForeignKey(nameof(ClientId))]
    public UserEntity ClientEntity { get; set; }

    public virtual IEnumerable<RoomReservationEntity> Rooms { get; set; }

    public virtual IEnumerable<AdditionalServiceReservationEntity> AdditionalServices { get; set; }

    //llaves foraneas auditoria
    public virtual UserEntity CreatedByUser { get; set; }

    public virtual UserEntity UpdatedByUser { get; set; }
}
