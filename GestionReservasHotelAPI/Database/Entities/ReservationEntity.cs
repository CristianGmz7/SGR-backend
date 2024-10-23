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

    // TODO: definir la relacion entre usuario y reservacion
    [Column("client_id")]
    public string ClientId { get; set; }

    public virtual IEnumerable<RoomReservationEntity> Rooms { get; set; }

    public virtual IEnumerable<AdditionalServiceReservationEntity> AdditionalServices { get; set; }
}
