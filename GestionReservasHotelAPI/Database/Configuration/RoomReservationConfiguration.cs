using GestionReservasHotelAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionReservasHotelAPI.Database.Configuration;

public class RoomReservationConfiguration : IEntityTypeConfiguration<RoomReservationEntity>
{
    public void Configure(EntityTypeBuilder<RoomReservationEntity> builder)
    {
        //cuando se este creando
        builder.HasOne((e) => e.CreatedByUser)      //propiedad virtual
            .WithMany()
            .HasForeignKey((e) => e.CreatedBy)       //CreatedBy es la llave foranea
            .HasPrincipalKey((e) => e.Id);           //esto representa la tabla de usuarios
                                                     //.IsRequired();

        //cuando se este editando
        builder.HasOne((e) => e.UpdatedByUser)      //propiedad virtual
            .WithMany()
            .HasForeignKey((e) => e.UpdatedBy)       //CreatedBy es la llave foranea
            .HasPrincipalKey((e) => e.Id);           //esto representa la tabla de usuarios
                                                     //.IsRequired();
    }
}
