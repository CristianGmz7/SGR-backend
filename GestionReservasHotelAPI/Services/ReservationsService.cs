using AutoMapper;
using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.AdditionalServices;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Hotels;
using GestionReservasHotelAPI.Dtos.Reservations;
using GestionReservasHotelAPI.Dtos.Rooms;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GestionReservasHotelAPI.Services;

public class ReservationsService : IReservationsService
{
    private readonly GestionReservasHotelContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ReservationsService> _logger;
    private readonly IAuthService _authService;
    private readonly int PAGE_SIZE;

    public ReservationsService(GestionReservasHotelContext context, 
        IMapper mapper, 
        ILogger<ReservationsService> logger,
        IAuthService authService,
        IConfiguration configuration
        )
    {
        this._context = context;
        this._mapper = mapper;
        this._logger = logger;
        this._authService = authService;
        PAGE_SIZE = configuration.GetValue<int>("Pagination:ReservationPageSize");

    }

    //supongo que se necesita string de id de cliente
    //Este metodo se implementará en SideBar Ver Reservas Hechas. NO ES el que se muestra
    //cuando selecciona habitaciones y le da confirmar
    public async Task<ResponseDto<PaginationDto<List<ReservationDto>>>> GetReservationListAsync(
        string clientId = "", int page = 1)
    {

        clientId = _authService.GetUserId();        //esta de manera temporal mientras se agregan usuarios
                                            //debe hacerse verificacion que el usuario exista
        int startIndex = (page - 1) * PAGE_SIZE;
        var reservationEntityQuery = _context.Reservations
            .Include(x => x.Rooms)
                .ThenInclude(room => room.Room)
                    .ThenInclude(room => room.Hotel)        //añadido para acceder props del Hotel
            .Include(x => x.AdditionalServices)
                .ThenInclude(aS => aS.AdditionalService)
            .Where(x => x.ClientId == clientId);

        int totalReservations = await reservationEntityQuery.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalReservations / PAGE_SIZE);

        var reservationsEntity = await reservationEntityQuery
            .OrderByDescending(x => x.FinishDate)
            .Skip(startIndex)
            .Take(PAGE_SIZE)
            .ToListAsync();

        var reservationsDto = reservationsEntity.Select(reservation => new ReservationDto
        {
            Id = reservation.Id,
            StartDate = reservation.StartDate,
            FinishDate = reservation.FinishDate,
            //DEBE DE CALCULARSE LA CONDICIÓN DE LA RESERVA EN BASE A LA FECHA ACTUAL Y FECHA FINAL
            Condition = DateTime.Now < reservation.FinishDate ? "CONFIRMADA" : "COMPLETADA",
            Price = reservation.Price,
            ClientId = clientId,            //considerar si requiere porque ya se sabe que es del usuario
            
            RoomsInfoList = reservation.Rooms.Select(rR => new RoomDto
            {
                //estos son los campos que se necesitan para el frontend
                Id = rR.Room.Id,
                NumberRoom = rR.Room.NumberRoom,
                TypeRoom = rR.Room.TypeRoom,
                PriceNight = rR.Room.PriceNight,
                ImageUrl = rR.Room.ImageUrl,
                HotelInfo = new HotelDto
                {
                    Id = rR.Room.Hotel.Id,
                    Name = rR.Room.Hotel.Name
                }
            }).ToList(),
            AdditionalServicesInfoList = reservation.AdditionalServices.Select(aS => new AdditionalServiceDto
            {
                Id = aS.AdditionalService.Id,
                Name = aS.AdditionalService.Name,
                Price = aS.AdditionalService.Price,
            }).ToList()
        }
        ).ToList();

        return new ResponseDto<PaginationDto<List<ReservationDto>>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Reservas encontradas exitosamente",
            Data = new PaginationDto<List<ReservationDto>>
            {
                CurrentPage = page,
                PageSize = PAGE_SIZE,
                TotalItems = totalReservations,
                TotalPages = totalPages,
                Items = reservationsDto,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages,
            }
        };

    }

    //metodo que obtiene lista de reservas por rango de fechas
    //este metodo será implementando posteriormente
    public async Task<ResponseDto<PaginationDto<List<ReservationDto>>>> GetReservationListBetweenDates(
        string clientId = "", int page = 1, DateTime filterStartDate = default, 
        DateTime filterEndDate = default)
    {

        if(filterStartDate == default)
        {
            filterStartDate = DateTime.Now;
        }
        if(filterEndDate == default)
        {
            filterEndDate = DateTime.Now.AddDays(1);
        }
        if(filterEndDate < filterStartDate)
        {
            return new ResponseDto<PaginationDto<List<ReservationDto>>>
            {
                StatusCode = 404,
                Status = false,
                Message = "La fecha de fin del filtro debe ser mayor que la fecha de inicio de filtro"
            };
        }

        clientId = _authService.GetUserId();

        int startIndex = (page - 1) * PAGE_SIZE;

        //AQUI TENGO UN DILEMA CON EL FRONTEND: El Query sera estatico o se ira actualizando
        //cuando se le filtrar y haya nuevo rango de fechas

        var resevartionEntityQuery = _context.Reservations
            .Include(x => x.Rooms)
                .ThenInclude(room => room.Room)
                    .ThenInclude(room => room.Hotel)
            .Include(x => x.AdditionalServices)
                .ThenInclude(aS => aS.AdditionalService)
            .Where(reserv => reserv.ClientId == clientId && (
                (reserv.StartDate >= filterStartDate && reserv.StartDate <= filterEndDate) &&
                (reserv.FinishDate >= filterStartDate && reserv.FinishDate <= filterEndDate)
            ));       //el operador && es en el primer caso que se planteó de la logica
        
        int totalReservations = await resevartionEntityQuery.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalReservations / PAGE_SIZE);

        var reservationsEntity = await resevartionEntityQuery
            .OrderByDescending(x => x.FinishDate)
            .Skip(startIndex)
            .Take(PAGE_SIZE)
            .ToListAsync();

        var reservationsDto = reservationsEntity.Select(reservation => new ReservationDto
        {
            Id = reservation.Id,
            StartDate = reservation.StartDate,
            FinishDate = reservation.FinishDate,
            Condition = DateTime.Now < reservation.FinishDate ? "CONFIRMADA" : "COMPLETADA",
            Price = reservation.Price,
            ClientId = clientId,

            RoomsInfoList = reservation.Rooms.Select(rR => new RoomDto
            {
                //estos son los campos que se necesitan para el frontend
                Id = rR.Room.Id,
                NumberRoom = rR.Room.NumberRoom,
                TypeRoom = rR.Room.TypeRoom,
                PriceNight = rR.Room.PriceNight,
                ImageUrl = rR.Room.ImageUrl,
                HotelInfo = new HotelDto
                {
                    Id = rR.Room.Hotel.Id,
                    Name = rR.Room.Hotel.Name
                }
            }).ToList(),
            AdditionalServicesInfoList = reservation.AdditionalServices.Select(aS => new AdditionalServiceDto
            {
                Id = aS.AdditionalService.Id,
                Name = aS.AdditionalService.Name,
                Price = aS.AdditionalService.Price,
            }).ToList()

        }
        ).ToList();

        return new ResponseDto<PaginationDto<List<ReservationDto>>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Reservas encontradas exitosamente",
            Data = new PaginationDto<List<ReservationDto>>
            {
                CurrentPage = page,
                PageSize = PAGE_SIZE,
                TotalItems = totalReservations,
                TotalPages = totalPages,
                Items = reservationsDto,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            }
        };
    }

    //DTO de respuesta actualizado
    public async Task<ResponseDto<ReservationDto>> GetReservationByIdAsync (Guid id)
    {
        var reservationEntity = await _context.Reservations
            .Include(x => x.Rooms)
                .ThenInclude(x => x.Room)
                    .ThenInclude(room => room.Hotel)
            .Include(x => x.AdditionalServices)
                .ThenInclude(x => x.AdditionalService)
            .FirstOrDefaultAsync(x => x.Id == id);

        if(reservationEntity == null)
        {
            return new ResponseDto<ReservationDto>
            {
                StatusCode = 404,
                Status = false,
                Message = $"La reservacion {id} no existe"
            };
        }

        var reservationDto = new ReservationDto
        {
            Id = reservationEntity.Id,
            StartDate = reservationEntity.StartDate,
            FinishDate = reservationEntity.FinishDate,
            //DEBE DE CALCULARSE LA CONDICIÓN DE LA RESERVA EN BASE A LA FECHA ACTUAL Y FECHA FINAL
            Condition = DateTime.Now < reservationEntity.FinishDate ? "CONFIRMADA" : "COMPLETADA",
            Price = reservationEntity.Price,
            ClientId = reservationEntity.ClientId,

            RoomsInfoList = reservationEntity.Rooms.Select(rR => new RoomDto
            {
                Id = rR.Room.Id,
                NumberRoom = rR.Room.NumberRoom,
                TypeRoom = rR.Room.TypeRoom,
                PriceNight = rR.Room.PriceNight,
                ImageUrl = rR.Room.ImageUrl,
                HotelInfo = new HotelDto
                {
                    Id = rR.Room.Hotel.Id,
                    Name = rR.Room.Hotel.Name
                }
            }).ToList(),
            AdditionalServicesInfoList = reservationEntity.AdditionalServices.Select(aS => new AdditionalServiceDto
            {
                Id = aS.AdditionalService.Id,
                Name = aS.AdditionalService.Name,
                Price = aS.AdditionalService.Price,
            }).ToList()
        };

        return new ResponseDto<ReservationDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Reservacion encontrado exitosamente",
            Data = reservationDto
        };
    }

    public async Task<ResponseDto<ReservationDto>> CreateReservationAsync (ReservationCreateDto dto)
    {
        using(var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                //posible solucion si llega a dar problemas, validar solo las fechas sin tomar en cuenta la hora
                if (dto.StartDate < DateTime.Now)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Error, la fecha inicial debe ser mayor o igual que la actual"
                    };
                }

                if(dto.FinishDate < dto.StartDate)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Error, la fecha inicial debe ser menor que la fecha final"
                    };
                }

                if (!dto.RoomsList.Any())
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Error, debe enviar al menos una habitacion para crear la reserva"
                    };
                }

                var roomIds = dto.RoomsList.Select(Guid.Parse).ToList();
                var roomsEntity = await _context.Rooms.Where(r => roomIds.Contains(r.Id)).ToListAsync();

                if (roomsEntity.Count != roomIds.Count)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "Error, una o más habitaciones no existen"
                    };
                }

                //Metodo implementando para verificar que las habitaciones sean del mismo hotel
                var hotelId = roomsEntity.First().HotelId;  // Suponiendo que cada habitación tiene un HotelId
                if (roomsEntity.Any(r => r.HotelId != hotelId))
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Error, todas las habitaciones deben pertenecer al mismo hotel"
                    };
                }
                //Fin de metodo implementando para verificar que las habitaciones sean del mismo hotel

                //validar que las habitaciones seleccionados no tengan otras reservas
                foreach (var room in roomsEntity)
                {
                    var overlappingReservations = await _context.RoomReservations
                        .Where(rr => rr.RoomId == room.Id && rr.Reservation.FinishDate >= DateTime.Now)
                        .Select(rr => rr.Reservation)
                        .Where(r => (dto.StartDate >= r.StartDate && dto.StartDate <= r.FinishDate) ||
                                    (dto.FinishDate >= r.StartDate && dto.FinishDate <= r.FinishDate))
                        .ToListAsync();

                    if (overlappingReservations.Any())
                    {
                        return new ResponseDto<ReservationDto>
                        {
                            StatusCode = 400,
                            Status = false,
                            Message = $"Error, la habitacion {room.NumberRoom} no está disponible para las fechas seleccionadas"
                        };
                    }
                }

                double additionalServicesTotal = 0;
                var additionalServicesEntity = new List<AdditionalServiceEntity>();

                //Calcular los dias que sera la reserva
                var reservationDays = (dto.FinishDate - dto.StartDate).Days;
                if(reservationDays <= 1)
                {
                    reservationDays = 1;
                }

                //se crea additionalServiceIds fuera del if para hacer la evaluacion cuando se quiera
                //agregar a la base de datos
                List<Guid> additionalServicesIds = new List<Guid>();

                if (dto.AdditionalServicesList.Any() && dto.AdditionalServicesList != null)
                {
                    additionalServicesIds = dto.AdditionalServicesList.Select(Guid.Parse).ToList();
                    additionalServicesEntity = await _context.AdditionalServices.Where(aS => additionalServicesIds.Contains(aS.Id)).ToListAsync();

                    if (additionalServicesEntity.Count != additionalServicesIds.Count)
                    {
                        return new ResponseDto<ReservationDto>
                        {
                            StatusCode = 404,
                            Status = false,
                            Message = "Error, uno o más servicios adicionales no existen"
                        };

                    }

                    //Metodo implementando para verificar que los SA sean del mismo hotel
                    if (additionalServicesEntity.Any(aS => aS.HotelId != hotelId))
                    {
                        return new ResponseDto<ReservationDto>
                        {
                            StatusCode = 400,
                            Status = false,
                            Message = "Error, todos los servicios adicionales deben pertenecer al mismo hotel que las habitaciones seleccionadas"
                        };
                    }
                    //Fin Metodo implementando para verificar que las SA sean del mismoD hotel
                    
                    additionalServicesTotal = reservationDays * additionalServicesEntity.Sum(aS => aS.Price);

                }   

                double roomsTotal = reservationDays * roomsEntity.Sum(r => r.PriceNight);
                double totalAmount = roomsTotal + additionalServicesTotal;

                //A este punto ya puede cargarse un nuevo ReservationEntity

                var reservationEntity = new ReservationEntity
                {
                    StartDate = dto.StartDate,
                    FinishDate = dto.FinishDate,
                    //Estado de reserva / Condition se elimino
                    Price = totalAmount,
                    //Los List IEnumerable no se colocan
                };

                reservationEntity.ClientId = _authService.GetUserId();

                // Agregar la reserva al contexto y guardar los cambios
                _context.Reservations.Add(reservationEntity);
                await _context.SaveChangesAsync();

                //Guardar cambio de habitaciones en la base de datos
                await _context.SaveChangesAsync();

                // Crear y agregar las relaciones de RoomReservations
                var roomsReservationsEntity = roomIds.Select(room => new RoomReservationEntity
                {
                    ReservationId = reservationEntity.Id,
                    RoomId = room
                }); 

                _context.RoomReservations.AddRange(roomsReservationsEntity);
                await _context.SaveChangesAsync();


                // Crear y agregar las relaciones de AdditionalServiceReservation
                if(additionalServicesIds.Any() && additionalServicesEntity != null)
                {
                    var additionalServiceReservations = additionalServicesIds.Select(aS => new AdditionalServiceReservationEntity
                    {
                        ReservationId = reservationEntity.Id,
                        AdditionalServiceId = aS
                    });

                    _context.AdditionalServiceReservations.AddRange(additionalServiceReservations);
                    await _context.SaveChangesAsync();
                }

                //LANZAR ERROR DE PRUEBA: Si funciona
                //throw new Exception("Error para probar el rollback");

                // Confirmar la transacción
                await transaction.CommitAsync();

                // Volver a cargar la reserva desde la base de datos, incluyendo las demas tablas
                var loadedReservationEntity = await _context.Reservations
                    .Include(reserv => reserv.Rooms)
                        .ThenInclude(rR => rR.Room)
                            .ThenInclude(room => room.Hotel)
                    .Include(reserv => reserv.AdditionalServices)
                        .ThenInclude(aS => aS.AdditionalService)
                .FirstOrDefaultAsync(reserv => reserv.Id == reservationEntity.Id);

                //por si existe algun problema en cargar la reserva
                if (loadedReservationEntity == null)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "Error al cargar la reserva recién creada"
                    };
                }

                // Mapear la entidad de reserva a un DTO y retornar la respuesta
                var reservationDto = new ReservationDto
                {
                    Id = loadedReservationEntity.Id,
                    StartDate = loadedReservationEntity.StartDate,
                    FinishDate = loadedReservationEntity.FinishDate,
                    Condition = DateTime.Now < loadedReservationEntity.FinishDate ? "CONFIRMADA" : "COMPLETADA",
                    Price = loadedReservationEntity.Price,
                    ClientId = loadedReservationEntity.ClientId,

                    RoomsInfoList = loadedReservationEntity.Rooms.Select(rR => new RoomDto
                    {
                        Id = rR.Room.Id,
                        NumberRoom = rR.Room.NumberRoom,
                        TypeRoom = rR.Room.TypeRoom,
                        PriceNight = rR.Room.PriceNight,
                        ImageUrl = rR.Room.ImageUrl,
                        HotelInfo = new HotelDto
                        {
                            Id = rR.Room.Hotel.Id,
                            Name = rR.Room.Hotel.Name
                        }
                    }).ToList(),
                    AdditionalServicesInfoList = loadedReservationEntity.AdditionalServices.Select(aS => new AdditionalServiceDto
                    {
                        Id = aS.AdditionalService.Id,
                        Name = aS.AdditionalService.Name,
                        Price = aS.AdditionalService.Price,
                    }).ToList()
                };


                return new ResponseDto<ReservationDto>
                {
                    StatusCode = 201,
                    Status = true,
                    Message = "Reservación creada exitosamente",
                    Data = reservationDto
                };


            }   //fin try
            catch(Exception e)
            {
                await transaction.RollbackAsync();

                _logger.LogError(e, "Error al crear la reservacion");

                return new ResponseDto<ReservationDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo error al crear la reservacion"
                };
            }   //fin catch
        }   //fin de using
    }   //fin de metodo CreateReservationAsync

    public async Task<ResponseDto<ReservationDto>> EditReservationAsync (ReservationEditDto dto, Guid id)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                //verificar que la reserva exista
                var reservationEntity = await _context.Reservations
                    .Include(reserv => reserv.Rooms)
                        .ThenInclude(rR => rR.Room)
                            .ThenInclude(room => room.Hotel)
                    .Include(reserv => reserv.AdditionalServices)
                        .ThenInclude(aS => aS.AdditionalService)
                    .FirstOrDefaultAsync(reserv => reserv.Id == id);
                if (reservationEntity == null)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "La reserva no existe"
                    };
                }

                //Verificar error que la fecha actual se encuentre en el rango de fechas de la reserva
                if(DateTime.Now >= reservationEntity.StartDate && DateTime.Now <= reservationEntity.FinishDate)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "La reserva no puede ser modificada porque se encuentra en proceso"
                    };
                }

                //Verificar error que la fecha actual sea mayor que la fecha de fin de la reserva
                if(DateTime.Now > reservationEntity.FinishDate)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "La reserva no puede ser modificada porque ya expiro"
                    };
                }

                //POSIBLE VERIFICACION de ver estado de reserva: Cancelada / No cancelada

                if (dto.StartDate < DateTime.Now)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Error, la fecha inicial debe ser mayor o igual que la actual"
                    };
                }

                if (dto.FinishDate < dto.StartDate)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Error, la fecha inicial debe ser menor que la fecha final"
                    };
                }

                if (!dto.RoomsList.Any())
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Error, debe enviar al menos una habitacion para crear la reserva"
                    };
                }

                //Pasar el string del dto a Un guid Id
                var roomIds = dto.RoomsList.Select(Guid.Parse).ToList();

                //Estos son los registros que ya existian y se borraran de tabla RoomReservations
                var existingRoomReservations = await _context.RoomReservations
                    .Where(r => r.ReservationId == id)
                    .ToListAsync();

                //guardar id's de habitaciones de la reserva original
                var originalRoomIds = existingRoomReservations.Select(r => r.RoomId).ToList();

                //en base a las habitaciones de la reserva original verificar cuales son las nuevas habitaciones
               var newRoomIds = roomIds.Except(originalRoomIds).ToList();

                //determinar cuales son las habitaciones que deben removerse
                var removedRoomIds = originalRoomIds.Except(roomIds).ToList();

                //determinar habitaciones que aun quedan de la reserva original
                var existingRoomIds = originalRoomIds.Except(removedRoomIds).ToList();

                //union de las habitaciones que aun existen en la reservas mas las nuevas
                var allRoomsIds = existingRoomIds.Concat(newRoomIds).ToList();

                //verificar que las habitaciones de la nueva reserva existan
                var roomsEntity = await _context.Rooms.Where(r => allRoomsIds.Contains(r.Id)).ToListAsync();

                if (roomsEntity.Count != allRoomsIds.Count)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "Error, una o más habitaciones no existen"
                    };
                }

                //verificar que las habitaciones sean del mismo hotel
                var hotelId = roomsEntity.First().HotelId;

                if (roomsEntity.Any(r => r.HotelId != hotelId))
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Error, todas las habitaciones deben pertenecer al mismo hotel"
                    };
                }

                //validar que las habitaciones de la reserva no tengan otras reservas
                //este metodo no es igual al de crear debido a la ultima condicion (...&& rr.Reservation.Id != id)
                foreach (var room in roomsEntity)
                {
                    var overlappingReservations = await _context.RoomReservations
                        .Where(rr => rr.RoomId == room.Id && rr.Reservation.FinishDate >= DateTime.Now && rr.Reservation.Id != id)
                        .Select(rr => rr.Reservation)
                        .Where(r => (dto.StartDate >= r.StartDate && dto.StartDate <= r.FinishDate) ||
                                    (dto.FinishDate >= r.StartDate && dto.FinishDate <= r.FinishDate))
                        .ToListAsync();

                    if (overlappingReservations.Any())
                    {
                        return new ResponseDto<ReservationDto>
                        {
                            StatusCode = 400,
                            Status = false,
                            Message = $"Error, la habitacion {room.NumberRoom} no está disponible para las fechas seleccionadas"
                        };
                    }
                }

                //Pasar el string del dto a Un guid Id
                var additionalServicesIds = dto.AdditionalServicesList.Select(Guid.Parse).ToList();

                //Estos son los registros que ya existian y se borraran de tabla AdditionalServicesReservation
                var existingAdditionalServicesReservations = await _context.AdditionalServiceReservations
                    .Where(aS => aS.ReservationId == id)
                    .ToListAsync();

                //guardar id's de SA de la reserva original
                var originalAdditionalServiceIds = existingAdditionalServicesReservations.Select(aS => aS.Id).ToList();

                //en base a los SA de la reserva original verificar cuales son los nuevos SA
                var newAdditionalServiceIds = additionalServicesIds.Except(originalAdditionalServiceIds).ToList();

                //determinar cuales son los SA que deben removerse
                var removeAdditionalServiceIds = originalAdditionalServiceIds.Except(additionalServicesIds).ToList();

                //determinar SA que aun quedan de la reserva original
                var existingAdditionalServiceIds = originalAdditionalServiceIds.Except(removeAdditionalServiceIds).ToList();

                //union de las habitaciones que aun existen en la reservas mas las nuevas
                var allAdditionalServiceIds = existingAdditionalServiceIds.Concat(newAdditionalServiceIds).ToList();

                double additionalServicesTotal = 0;
                var additionalServicesEntity = new List<AdditionalServiceEntity>();

                //Calcular los dias que sera la reserva
                var reservationDays = (dto.FinishDate - dto.StartDate).Days;
                if (reservationDays <= 1)
                {
                    reservationDays = 1;
                }

                if (dto.AdditionalServicesList.Any() && dto.AdditionalServicesList != null)
                {
                    additionalServicesEntity = await _context.AdditionalServices
                        .Where(aS => additionalServicesIds
                        .Contains(aS.Id))
                        .ToListAsync();

                    if (additionalServicesEntity.Count != additionalServicesIds.Count)
                    {
                        return new ResponseDto<ReservationDto>
                        {
                            StatusCode = 404,
                            Status = false,
                            Message = "Error, uno o más servicios adicionales no existen"
                        };

                    }

                    //Metodo implementando para verificar que los SA sean del mismo hotel
                    if (additionalServicesEntity.Any(aS => aS.HotelId != hotelId))
                    {
                        return new ResponseDto<ReservationDto>
                        {
                            StatusCode = 400,
                            Status = false,
                            Message = "Error, todos los servicios adicionales deben pertenecer al mismo hotel que las habitaciones seleccionadas"
                        };
                    }
                    //Fin Metodo implementando para verificar que las SA sean del mismo hotel

                    additionalServicesTotal = reservationDays * additionalServicesEntity.Sum(aS => aS.Price);
                }

                double roomsTotal = reservationDays * roomsEntity.Sum(r => r.PriceNight);
                double totalAmount = roomsTotal + additionalServicesTotal;

                //actualizar los datos de la reserva

                reservationEntity.StartDate = dto.StartDate;
                reservationEntity.FinishDate = dto.FinishDate;
                reservationEntity.Price = totalAmount;  
                reservationEntity.ClientId = _authService.GetUserId();

                _context.Reservations.Update(reservationEntity);
                await _context.SaveChangesAsync();

                //Eliminar habitaciones de la antigua reservacion 
                _context.RoomReservations.RemoveRange(existingRoomReservations);
                await _context.SaveChangesAsync();

                //Asignar las habitaciones con la reservacion en la tabla RoomReservations
                var roomsReservationNew = allRoomsIds
                    .Select(room => new RoomReservationEntity
                    {
                        ReservationId = reservationEntity.Id,
                        RoomId = room
                    })
                    .ToList();

                _context.RoomReservations.AddRange(roomsReservationNew);
                await _context.SaveChangesAsync();

                //Eliminar servicios adicionales de la antigua reservacion 
                _context.AdditionalServiceReservations.RemoveRange(existingAdditionalServicesReservations);
                await _context.SaveChangesAsync();

                //Asignar las habitaciones con la reservacion en la tabla RoomReservations
                var additionalServicesReservationNew = allAdditionalServiceIds
                    .Select(aS => new AdditionalServiceReservationEntity
                    {
                        ReservationId = reservationEntity.Id,
                        AdditionalServiceId = aS
                    })
                    .ToList();

                _context.AdditionalServiceReservations.AddRange(additionalServicesReservationNew);
                await _context.SaveChangesAsync();

                //ERROR DE PRUEBA
                //throw new Exception("Error para validar el rollback");
                await transaction.CommitAsync();

                //retornar respuesta
                var reservationDto = new ReservationDto
                {
                    Id = reservationEntity.Id,
                    StartDate = reservationEntity.StartDate,
                    FinishDate = reservationEntity.FinishDate,
                    Condition = DateTime.Now < reservationEntity.FinishDate ? "CONFIRMADA" : "COMPLETADA",
                    Price = reservationEntity.Price,
                    ClientId = reservationEntity.ClientId,

                    RoomsInfoList = reservationEntity.Rooms.Select(rR => new RoomDto
                    {
                        Id = rR.Room.Id,
                        NumberRoom = rR.Room.NumberRoom,
                        TypeRoom = rR.Room.TypeRoom,
                        PriceNight = rR.Room.PriceNight,
                        ImageUrl = rR.Room.ImageUrl,
                        HotelInfo = new HotelDto
                        {
                            Id = rR.Room.Hotel.Id,
                            Name = rR.Room.Hotel.Name
                        }
                    }).ToList(),
                    AdditionalServicesInfoList = reservationEntity.AdditionalServices.Select(aS => new AdditionalServiceDto
                    {
                        Id = aS.AdditionalService.Id,
                        Name = aS.AdditionalService.Name,
                        Price = aS.AdditionalService.Price,
                    }).ToList()
                };

                return new ResponseDto<ReservationDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Reservacion editada correctamente",
                    Data = reservationDto
                };

            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                _logger.LogError(e, "Error al editar la reservacion");

                return new ResponseDto<ReservationDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo error al editar la reservacion"
                };
            }   //fin del catch
        }   //fin del using
    }   //fin de metodo EditReservationAsync

    public async Task<ResponseDto<ReservationDto>> DeleteReservationAsync(Guid id)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var reservationEntity = await _context.Reservations.FindAsync(id);

                if(reservationEntity is null)
                {
                    return new ResponseDto<ReservationDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "La reservacion no existe"
                    };
                }

                _context.Reservations.Remove(reservationEntity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResponseDto<ReservationDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Reservacion eliminada correctamente"
                };

            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Error al borrar la reservacion");

                return new ResponseDto<ReservationDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Error al borrar la reservacion"
                };
            }
        }
    }

}
