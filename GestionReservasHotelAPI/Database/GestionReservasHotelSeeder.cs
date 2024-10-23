using GestionReservasHotelAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GestionReservasHotelAPI.Database
{
    public class GestionReservasHotelSeeder
    {
        public static async Task LoadDataAsync(
            GestionReservasHotelContext context,
            ILoggerFactory loggerFactory
            )
        {
            try
            {
                await LoadHotelsAsync(loggerFactory, context);
                await LoadRoomsAsync(loggerFactory, context);
                //await LoadReservationsAsync(loggerFactory, context);
                await LoadAdditionalsServicesAsync(loggerFactory, context);
                //await LoadRoomsReservationsAsync(loggerFactory, context);
                //await LoadAdditionalsServicesReservationsAsync(loggerFactory, context);
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<GestionReservasHotelSeeder>();
                logger.LogError(e, "Error iniciando la data del API");
            }
        }

        public static async Task LoadHotelsAsync(ILoggerFactory loggerFactory, GestionReservasHotelContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/hotels.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var hotels = JsonConvert.DeserializeObject<List<HotelEntity>>(jsonContent);

                if (!await context.Hotels.AnyAsync())
                {
                    context.AddRange(hotels);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<GestionReservasHotelSeeder>();
                logger.LogError(e, "Error al ejecutar el seed de hoteles");
            }
        }

        public static async Task LoadRoomsAsync(ILoggerFactory loggerFactory, GestionReservasHotelContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/rooms.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var rooms = JsonConvert.DeserializeObject<List<RoomEntity>>(jsonContent);

                if (!await context.Rooms.AnyAsync())
                {
                    context.AddRange(rooms);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<GestionReservasHotelSeeder>();
                logger.LogError(e, "Error al ejecutar el seed de habitaciones");
            }
        }

        public static async Task LoadReservationsAsync(ILoggerFactory loggerFactory, GestionReservasHotelContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/reservations.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var reservations = JsonConvert.DeserializeObject<List<ReservationEntity>>(jsonContent);

                if (!await context.Reservations.AnyAsync())
                {
                    context.AddRange(reservations);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<GestionReservasHotelSeeder>();
                logger.LogError(e, "Error al ejecutar el seed de reservaciones");
            }
        }

        public static async Task LoadAdditionalsServicesAsync(ILoggerFactory loggerFactory, GestionReservasHotelContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/additionals_services.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var additionalsServices = JsonConvert.DeserializeObject<List<AdditionalServiceEntity>>(jsonContent);

                if (!await context.AdditionalServices.AnyAsync())
                {
                    context.AddRange(additionalsServices);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<GestionReservasHotelSeeder>();
                logger.LogError(e, "Error al ejecutar el seed de servicios adicionales");
            }
        }

        public static async Task LoadRoomsReservationsAsync(ILoggerFactory loggerFactory, GestionReservasHotelContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/rooms_reservations.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var roomsReservations = JsonConvert.DeserializeObject<List<RoomReservationEntity>>(jsonContent);

                if (!await context.RoomReservations.AnyAsync())
                {
                    context.AddRange(roomsReservations);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<GestionReservasHotelSeeder>();
                logger.LogError(e, "Error al ejecutar el seed de reserva de habitaciones");
            }
        }

        public static async Task LoadAdditionalsServicesReservationsAsync(ILoggerFactory loggerFactory, GestionReservasHotelContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/additionals_services_reservations.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var additionalsServicesReservations = JsonConvert.DeserializeObject<List<AdditionalServiceReservationEntity>>(jsonContent);

                if (!await context.AdditionalServiceReservations.AnyAsync())
                {
                    context.AddRange(additionalsServicesReservations);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<GestionReservasHotelSeeder>();
                logger.LogError(e, "Error al ejecutar el seed de servicio adicional de reserva");
            }
        }

        //TODO: Terminar de agragar los metodos faltantes 
        // TODO: Anadir el seeder al program

    }
}
