using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace GestionReservasHotelAPI.Database
{
    public class GestionReservasHotelSeeder
    {
        public static async Task LoadDataAsync(
            GestionReservasHotelContext context,
            ILoggerFactory loggerFactory,
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            try
            {
                await LoadRolesAndUsersAsync(userManager, roleManager, loggerFactory);
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

        public static async Task LoadRolesAndUsersAsync(
                UserManager<UserEntity> userManager,
                RoleManager<IdentityRole> roleManager,
                ILoggerFactory loggerFactory
            )
        {
            try
            {
                //creacion de roles
                if(!await roleManager.Roles.AnyAsync())
                {
                    await roleManager.CreateAsync(new IdentityRole(RolesConstant.PAGEADMIN));
                    await roleManager.CreateAsync(new IdentityRole(RolesConstant.HOTELADMIN));
                    await roleManager.CreateAsync(new IdentityRole(RolesConstant.USER));
                }

                //creacion de usuarios
                if(!await userManager.Users.AnyAsync())
                {
                    // administrador de la pagina
                    var userPageAdmin = new UserEntity
                    {
                        FirstName = "Administrador Pagina",
                        LastName = "Sistema Gestion Reservas Hotel",
                        Email = "pageadmin@sgr.com",
                        UserName = "pageadmin@sgr.com",
                        ProfilePictureUrl = "https://images.pexels.com/photos/415829/pexels-photo-415829.jpeg"
                    };

                    // administradores de los hoteles que vienen del seeder
                    var userHotelAdmin1 = new UserEntity
                    {
                        FirstName = "Administrador Hotel 1",
                        LastName = "Sistema Gestion Reservas Hotel",
                        Email = "hoteladmin1@sgr.com",
                        UserName = "hoteladmin1@sgr.com",
                        ProfilePictureUrl = "https://images.pexels.com/photos/1542085/pexels-photo-1542085.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1"
                    };
                    var userHotelAdmin2 = new UserEntity
                    {
                        FirstName = "Administrador Hotel 2",
                        LastName = "Sistema Gestion Reservas Hotel",
                        Email = "hoteladmin2@sgr.com",
                        UserName = "hoteladmin2@sgr.com",
                        ProfilePictureUrl = "https://images.pexels.com/photos/634021/pexels-photo-634021.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1"
                    };
                    var userHotelAdmin3 = new UserEntity
                    {
                        FirstName = "Administrador Hotel 3",
                        LastName = "Sistema Gestion Reservas Hotel",
                        Email = "hoteladmin3@sgr.com",
                        UserName = "hoteladmin3@sgr.com",
                        ProfilePictureUrl = "https://images.pexels.com/photos/1370750/pexels-photo-1370750.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1"
                    };
                    var userHotelAdmin4 = new UserEntity
                    {
                        FirstName = "Administrador Hotel 4",
                        LastName = "Sistema Gestion Reservas Hotel",
                        Email = "hoteladmin4@sgr.com",
                        UserName = "hoteladmin4@sgr.com",
                        ProfilePictureUrl = "https://images.pexels.com/photos/3785079/pexels-photo-3785079.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1"

                    };
                    var userHotelAdmin5 = new UserEntity
                    {
                        FirstName = "Administrador Hotel 5",
                        LastName = "Sistema Gestion Reservas Hotel",
                        Email = "hoteladmin5@sgr.com",
                        UserName = "hoteladmin5@sgr.com",
                        ProfilePictureUrl = "https://plus.unsplash.com/premium_photo-1689568126014-06fea9d5d341?q=80&w=1470&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                    };
                    var userHotelAdmin6 = new UserEntity
                    {
                        FirstName = "Administrador Hotel 6",
                        LastName = "Sistema Gestion Reservas Hotel",
                        Email = "hoteladmin6@sgr.com",
                        UserName = "hoteladmin6@sgr.com",
                        ProfilePictureUrl = "https://images.unsplash.com/photo-1605974710431-32715cc561b3?q=80&w=1470&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                    };

                    var normalUser = new UserEntity
                    {
                        FirstName = "User",
                        LastName = "Sistema Gestion Reservas Hotel",
                        Email = "user@sgr.com",
                        UserName = "user@sgr.com",
                        ProfilePictureUrl = "https://plus.unsplash.com/premium_photo-1689977927774-401b12d137d6?q=80&w=1470&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                    };

                    await userManager.CreateAsync(userPageAdmin, "Temporal01*");
                    await userManager.CreateAsync(userHotelAdmin1, "Temporal01*");
                    await userManager.CreateAsync(userHotelAdmin2, "Temporal01*");
                    await userManager.CreateAsync(userHotelAdmin3, "Temporal01*");
                    await userManager.CreateAsync(userHotelAdmin4, "Temporal01*");
                    await userManager.CreateAsync(userHotelAdmin5, "Temporal01*");
                    await userManager.CreateAsync(userHotelAdmin6, "Temporal01*");
                    await userManager.CreateAsync(normalUser, "Temporal01*");

                    await userManager.AddToRoleAsync(userPageAdmin, RolesConstant.PAGEADMIN);
                    await userManager.AddToRoleAsync(userHotelAdmin1, RolesConstant.HOTELADMIN);
                    await userManager.AddToRoleAsync(userHotelAdmin2, RolesConstant.HOTELADMIN);
                    await userManager.AddToRoleAsync(userHotelAdmin3, RolesConstant.HOTELADMIN);
                    await userManager.AddToRoleAsync(userHotelAdmin4, RolesConstant.HOTELADMIN);
                    await userManager.AddToRoleAsync(userHotelAdmin5, RolesConstant.HOTELADMIN);
                    await userManager.AddToRoleAsync(userHotelAdmin6, RolesConstant.HOTELADMIN);
                    await userManager.AddToRoleAsync(normalUser, RolesConstant.USER);
                }
            }
            catch ( Exception e )
            {
                var logger = loggerFactory.CreateLogger<GestionReservasHotelSeeder>();
                logger.LogError(e.Message);
            }
        }

        //comentar este metodo debido a la relacion que se implementó
        public static async Task LoadHotelsAsync(ILoggerFactory loggerFactory, GestionReservasHotelContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/hotels.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var hotels = JsonConvert.DeserializeObject<List<HotelEntity>>(jsonContent);

                if (!await context.Hotels.AnyAsync())
                {
                    //añadir un usuario adminhotel por hotel
                    // verificar si es necesario colocar el ? antes del .Id;
                    var hotelAdminRoleId = (await context.Roles.FirstOrDefaultAsync(r => r.Name == RolesConstant.HOTELADMIN)).Id;

                    if (hotelAdminRoleId == null)
                    {
                        throw new Exception("Rol HOTELADMIN no encontrado.");
                    }

                    var hotelAdmins = await context.Users
                        .Where(u => u.UserRoles.Any(ur => ur.RoleId == hotelAdminRoleId))
                        .ToListAsync();

                    if (hotelAdmins.Count < hotels.Count)
                    {
                        throw new Exception("No hay suficientes usuarios de HOTELADMIN para el numero de hoteles.");
                    }


                    for (int i = 0; i < hotels.Count; i++)
                    {
                        var adminUser = hotelAdmins[i];
                        hotels[i].AdminUserId = adminUser.Id;
                        hotels[i].CreatedBy = adminUser.Id;
                        hotels[i].CreatedDate = DateTime.Now;
                        hotels[i].UpdatedBy = adminUser.Id;
                        hotels[i].UpdatedDate = DateTime.Now;
                    }

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
                    //colocar en los campos de autoria el user que es admin del hotel a la que pertenece la habitacion

                    var hotelsWithAdmins = await context.Hotels
                        .Include(h => h.AdminUserEntity)
                        .ToDictionaryAsync(h => h.Id);

                    for(int i = 0; i < rooms.Count; i++)
                    {
                        if (hotelsWithAdmins.TryGetValue(rooms[i].HotelId, out var hotel))
                        {
                            rooms[i].CreatedBy = hotel.AdminUserId;
                            rooms[i].CreatedDate = DateTime.Now;
                            rooms[i].UpdatedBy = hotel.AdminUserId;
                            rooms[i].UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            throw new Exception($"Hotel con ID {rooms[i].HotelId} no encontrado para la habitación {rooms[i].NumberRoom}");
                        }
                    }

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

        //este metodo no se utiliza
        public static async Task LoadReservationsAsync(ILoggerFactory loggerFactory, GestionReservasHotelContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/reservations.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var reservations = JsonConvert.DeserializeObject<List<ReservationEntity>>(jsonContent);

                if (!await context.Reservations.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();





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
                    //colocar en los campos de autoria el user que es admin del hotel al que pertenece el SA
                    var hotelsWithAdmins = await context.Hotels
                        .Include(h => h.AdminUserEntity)
                        .ToDictionaryAsync(h => h.Id);

                    for (int i = 0; i < additionalsServices.Count; i++)
                    {
                        if (hotelsWithAdmins.TryGetValue(additionalsServices[i].HotelId, out var hotel))
                        {
                            additionalsServices[i].CreatedBy = hotel.AdminUserId;
                            additionalsServices[i].CreatedDate = DateTime.Now;
                            additionalsServices[i].UpdatedBy = hotel.AdminUserId;
                            additionalsServices[i].UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            throw new Exception($"Hotel con ID {additionalsServices[i].HotelId} no encontrado para el SA {additionalsServices[i].Name}");
                        }
                    }

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

        //este metodo no se utiliza
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

        //este metodo no se utiliza
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
