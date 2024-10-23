using System.ComponentModel.DataAnnotations.Schema;

namespace GestionReservasHotelAPI.Database.Entities;

[Table("additional_services_reservations", Schema = "dbo")]
public class AdditionalServiceReservationEntity : BaseEntity
{
    [Column("additional_service_id")]
    public Guid AdditionalServiceId { get; set; }


    [ForeignKey(nameof(AdditionalServiceId))]
    public virtual AdditionalServiceEntity AdditionalService { get; set; }

    [Column("reservation_id")]
    public Guid ReservationId { get; set; }


    [ForeignKey(nameof(ReservationId))]
    public virtual ReservationEntity Reservation { get; set; }
}
