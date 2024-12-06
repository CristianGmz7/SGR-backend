using GestionReservasHotelAPI.Database.Configuration;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Database;

public class GestionReservasHotelContext : IdentityDbContext<UserEntity>
{
    private readonly IAuditService _auditService;

    public GestionReservasHotelContext(
        DbContextOptions options,
        IAuditService auditService
        ) : base(options)
    {
        this._auditService = auditService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("security");

        modelBuilder.Entity<UserEntity>().ToTable("users");
        modelBuilder.Entity<IdentityRole>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("users_roles");

        //agregado para funcionalidad del seeder
        //Comentar despues de ejecutar el seeder y descomentar sobreescritura SaveChangesAsync Context
        // Se debe comentar también el metodo LoadHotelAsync del Seeder despues de ejecutarse
        modelBuilder.Entity<UserEntity>()
            .HasMany(u => u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        modelBuilder.Entity<IdentityRole>()
            .HasMany<IdentityUserRole<string>>()
            .WithOne()
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        // fin agregado para funcionalidad del seeder

        // renombrando el resto de tablas de Identity
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("users_claims");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("roles_claims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("users_logins");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("users_tokens");

        modelBuilder.ApplyConfiguration(new HotelConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new AdditionalServiceConfiguration());
        modelBuilder.ApplyConfiguration(new AdditionalServiceReservationConfiguration());
        modelBuilder.ApplyConfiguration(new RoomReservationConfiguration());
        modelBuilder.ApplyConfiguration(new HotelReactConfiguration());

        // Probar a dejar solo las del esquema DBO con .Cascade y las de security con .Restrict si da problemas el querer eliminar
        // reservas, editar reservas
        //Set FKs OnRestrict
        var eTypes = modelBuilder.Model.GetEntityTypes();
        foreach (var type in eTypes)
        {
            var foreingKeys = type.GetForeignKeys();

            foreach (var foreignKey in foreingKeys)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

    }

    //Descomentar sobreescritura SaveChangesAsync cuando se haga el seeder
    public override Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified
            ));

        foreach (var entry in entries)
        {
            var entity = entry.Entity as BaseEntity;
            if (entity != null)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedBy = _auditService.GetUserId();
                    entity.CreatedDate = DateTime.Now;
                }
                else
                {
                    entity.UpdatedBy = _auditService.GetUserId();
                    entity.UpdatedDate = DateTime.Now;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<HotelEntity> Hotels { get; set; }
    public DbSet<ReservationEntity> Reservations { get; set; }
    public DbSet<RoomEntity> Rooms { get; set; }
    public DbSet<AdditionalServiceEntity> AdditionalServices { get; set; }
    public DbSet<AdditionalServiceReservationEntity> AdditionalServiceReservations { get; set; }
    public DbSet<RoomReservationEntity> RoomReservations { get; set; }
    public DbSet<HotelReactEntity> HotelsReacts { get; set; }
}
