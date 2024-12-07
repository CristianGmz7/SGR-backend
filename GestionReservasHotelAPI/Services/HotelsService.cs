using AutoMapper;
using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Hotels;
using GestionReservasHotelAPI.Dtos.Users;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Services;

public class HotelsService : IHotelsService
{
    private readonly GestionReservasHotelContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<HotelsService> _logger;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IAuditService _auditService;
    private readonly int PAGE_SIZE;

    public HotelsService(
            GestionReservasHotelContext context, 
            IMapper mapper,
            IConfiguration configuration,
            ILogger<HotelsService> logger,
            UserManager<UserEntity> userManager,
            IAuditService auditService
        ) 
    {
        this._context = context;
        this._mapper = mapper;
        this._logger = logger;
        this._userManager = userManager;
        this._auditService = auditService;
        PAGE_SIZE = configuration.GetValue<int>("Pagination:HotelPageSize");
    }

    public async Task<ResponseDto<PaginationDto<List<HotelDto>>>> GetHotelsListAsync(
        string searchTerm = "",
        int page = 1,
        int starsNumber = 0,
        string department = "",
        string city = "",
        int minLikes = -1,
        int maxLikes = -1
        )
    {
        int startIndex = (page - 1) * PAGE_SIZE;

        // Filtro base que incluye todos los hoteles
        var hotelEntityQuery = _context.Hotels.AsQueryable();

        // Si searchTerm no está vacío, se agrega el filtro
        if (!string.IsNullOrEmpty(searchTerm))
        {
            hotelEntityQuery = hotelEntityQuery.Where(x =>
                (x.Name + " " + x.Description + " " + x.Overview + " " + x.Address)
                .ToLower().Contains(searchTerm.ToLower()));
        }

        //aqui van todas las validaciones de los filtros
        //filtrar los hoteles por los numeros de estrellas que existan en el query
        if (starsNumber != 0) { 
            hotelEntityQuery = hotelEntityQuery.Where(x => x.StarsMichelin == starsNumber); 
        }
        //filtrar los hoteles por departamento
        if (!string.IsNullOrEmpty(department))
        {
            hotelEntityQuery = hotelEntityQuery.Where(x => x.Department == department);
        }
        //filtrar los hoteles por ciudad
        if (!string.IsNullOrEmpty(city))
        {
            hotelEntityQuery = hotelEntityQuery.Where(x => x.City == city);
        }

        // Unir con las reacciones para calcular likes y dislikes
        var hotelWithReactsQuery = hotelEntityQuery
            .GroupJoin(
                _context.HotelsReacts,
                hotel => hotel.Id,
                react => react.HotelId,
                (hotel, reacts) => new
                {
                    Hotel = hotel,
                    TotalLikes = reacts.Count(r => r.Reaction),      // Total de likes
                    TotalDislikes = reacts.Count(r => !r.Reaction)   // Total de dislikes
                }
            );

        //filtrar por cantidad de likes
        if (minLikes != -1 && maxLikes != -1)
        {
            hotelWithReactsQuery = hotelWithReactsQuery
                .Where(x =>
                    (maxLikes == -2000 && x.TotalLikes >= 2001) ||  // Más de 2000 likes
                    (x.TotalLikes >= minLikes && x.TotalLikes <= maxLikes) // Rango definido
                );
        }

        int totalHotels = await hotelWithReactsQuery.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalHotels / PAGE_SIZE);

        // Paginación y ordenamiento
        var hotelsEntity = await hotelWithReactsQuery
            .OrderBy(x => x.Hotel.Name)             //aqui ya se podria ir ordenando por likes por ejemplo
            .Skip(startIndex)
            .Take(PAGE_SIZE)
            .ToListAsync();

        //var hotelsDtos = _mapper.Map<List<HotelDto>>(hotelsEntity);
        var hotelsDtos = hotelsEntity.Select(x => new HotelDto
        {
            Id = x.Hotel.Id,
            Name = x.Hotel.Name,
            Address = $"{x.Hotel.Address}, {x.Hotel.City}, {x.Hotel.Department}",
            StarsMichelin = x.Hotel.StarsMichelin,
            NumberPhone = x.Hotel.NumberPhone,
            Overview = x.Hotel.Overview,
            Description = x.Hotel.Description,
            ImageUrl = x.Hotel.ImageUrl,
            AdminUserId = x.Hotel.AdminUserId,
            TotalLikes = x.TotalLikes,
            TotalDislikes = x.TotalDislikes
        }).ToList();

        return new ResponseDto<PaginationDto<List<HotelDto>>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Lista de registro obtenida correctamente.",
            Data = new PaginationDto<List<HotelDto>>
            {
                CurrentPage = page,
                PageSize = PAGE_SIZE,
                TotalItems = totalHotels,
                TotalPages = totalPages,
                Items = hotelsDtos,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages,
            }
        };
    }


    public async Task<ResponseDto<HotelDto>> GetHotelByIdAsync(Guid id)
    {
        var hotelEntity = await _context.Hotels.FirstOrDefaultAsync(c => c.Id == id);
        if (hotelEntity == null) 
        {
            return new ResponseDto<HotelDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "No se encontro el registro."
            };
        }

        // Calcula las reacciones asociadas al hotel
        var reactions = await _context.HotelsReacts
            .Where(r => r.HotelId == id)
            .GroupBy(r => r.Reaction)
            .Select(group => new
            {
                Reaction = group.Key, // true = likes, false = dislikes
                Count = group.Count()
            })
            .ToListAsync();

        int totalLikes = reactions.FirstOrDefault(r => r.Reaction)?.Count ?? 0;
        int totalDislikes = reactions.FirstOrDefault(r => !r.Reaction)?.Count ?? 0;

        var hotelDto = _mapper.Map<HotelDto>(hotelEntity);
        hotelDto.TotalDislikes = totalDislikes;
        hotelDto.TotalLikes = totalLikes;

        return new ResponseDto<HotelDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registro encontrado.",
            Data = hotelDto
        };
    }

    public async Task<ResponseDto<HotelDto>> CreateAsync(HotelCreateDto dto)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {

            try
            {
                //validaciones acerca del usuario que será encargado del hotel

                //string adminHotelId = "00d3f5ce-36e7-4f60-a7e8-b791f7628da5";
                //var adminHotelRole = await _context.Roles.FindAsync(adminHotelId);
                var adminHotelRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RolesConstant.HOTELADMIN);

                if (adminHotelRole == null)
                {
                    return new ResponseDto<HotelDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "El rol no existe"
                    };
                }

                var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.AdminUserId);

                if (userEntity == null)
                {
                    return new ResponseDto<HotelDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "No se encontró el usuario"
                    };
                }

                //Eliminar registro anterior del rol antes de crear el nuevo (esto debido al efecto cascada)
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userEntity.Id)
                    .ToListAsync();

                _context.UserRoles.RemoveRange(userRoles);

                //Agregar el nuevo rol de administrador Hotel
                var roleResult = await _userManager.AddToRoleAsync(userEntity, adminHotelRole.Name);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new ResponseDto<HotelDto>
                    {
                        StatusCode = 500,
                        Status = false,
                        Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                    };
                }

                // TODO: Validar que el hotel no se repita. (este todo es viejo no tiene sentido)

                var hotelEntity = _mapper.Map<HotelEntity>(dto);

                _context.Hotels.Add(hotelEntity);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var hotelDto = _mapper.Map<HotelDto>(hotelEntity);

                return new ResponseDto<HotelDto>
                {
                    StatusCode = 201,
                    Status = true,
                    Message = "Resgistro creado exitosamente",
                    Data = hotelDto
                };
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Se produjo un error al crear el hotel.");
                return new ResponseDto<HotelDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo un error al crear el hotel."
                };
            }

        }

    }

    public async Task<ResponseDto<HotelDto>> EditAsync(HotelEditDto dto, Guid id)
    {

        var hotelEntity = await _context.Hotels.FirstOrDefaultAsync(c => c.Id == id);
        if (hotelEntity == null) 
        {
            return new ResponseDto<HotelDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "No se encontro el registro."
            };
        }

        //validacion de que el que quiera hacer el cambio sea el administrador del hotel y no otro
        //esto de manera similar debe hacerse para cuando se quiera modificar una habitación
        var hotelAdminId = _auditService.GetUserId();
        if (hotelAdminId != hotelEntity.AdminUserId)
        {
            return new ResponseDto<HotelDto>
            {
                StatusCode = 400,
                Status = false,
                Message = "Solo el administrador del hotel puede hacer cambios."
            };
        }

        _mapper.Map<HotelEditDto, HotelEntity>(dto, hotelEntity);

        _context.Hotels.Update(hotelEntity);

        await _context.SaveChangesAsync();

        var hotelDto = _mapper.Map<HotelDto>(hotelEntity);

        return new ResponseDto<HotelDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registro modificado correctamente.",
            Data = hotelDto
        };
    }

    public async Task<ResponseDto<HotelDto>> DeleteAsync(Guid id)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var hotelEntity = await _context.Hotels.FirstOrDefaultAsync(x => x.Id == id);
                if (hotelEntity == null)
                {
                    return new ResponseDto<HotelDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "No se encontro el registro."
                    };
                }

                //validacion acerca que no hayan reservas cuya fecha fin sea mayor a la actual
                bool hasActiveReservations = await _context.Reservations
                    .AnyAsync(r => r.Rooms.Any(rr => rr.Room.HotelId == id) && r.FinishDate >= DateTime.Now);

                if (hasActiveReservations)
                {
                    return new ResponseDto<HotelDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "No se puede eliminar el hotel ya que tiene reservas pendientes."
                    };
                }

                //cambiar el rol del que era admin hotel a user
                //string userId = "0416b6e2-509c-42e0-acf3-a1157e623d9b";
                //var userRole = await _context.Roles.FindAsync(userId);
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RolesConstant.USER);

                if (userRole == null)
                {
                    return new ResponseDto<HotelDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "El rol no existe"
                    };
                }

                var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == hotelEntity.AdminUserId);

                if (userEntity == null)
                {
                    return new ResponseDto<HotelDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "No se encontró el usuario"
                    };
                }

                //Eliminar registro anterior del rol antes de crear el nuevo (esto debido al efecto cascada)
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userEntity.Id)
                    .ToListAsync();

                _context.UserRoles.RemoveRange(userRoles);

                //Agregar el nuevo rol de usuario normal
                var roleResult = await _userManager.AddToRoleAsync(userEntity, userRole.Name);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new ResponseDto<HotelDto>
                    {
                        StatusCode = 500,
                        Status = false,
                        Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                    };
                }

                //AQUI INICIAN LAS ELIMINACIONES MANUALES DE LOS REGISTRO ASOCIADOS, todo esto debido a configuracion DeleteBehavior.Restrict  
                // 1. Eliminar registros asociados de las habitaciones del hotel en RoomsReservations
                var roomIds = await _context.Rooms
                    .Where(r => r.HotelId == id)
                    .Select(r => r.Id)
                    .ToListAsync();

                var roomReservations = await _context.RoomReservations
                    .Where(rr => roomIds.Contains(rr.RoomId))
                    .ToListAsync();

                _context.RoomReservations.RemoveRange(roomReservations);

                // 2. Eliminar registros asociados de los servicios adicionales del hotel en AdditionalServicesReservations
                var additionalServicesIds = await _context.AdditionalServices
                    .Where(aS => aS.HotelId == id)
                    .Select(aS => aS.Id)
                    .ToListAsync();

                var additionalServicesReservations = await _context.AdditionalServiceReservations
                    .Where(asr => additionalServicesIds.Contains(asr.AdditionalServiceId))
                    .ToListAsync();

                _context.AdditionalServiceReservations.RemoveRange(additionalServicesReservations);

                //3. Remover las habitaciones que pertenecian al hotel que se quiere eliminar 
                var hotelRooms = await _context.Rooms
                    .Where(r => r.HotelId == id)
                    .ToListAsync();
                //anteriormente solo se tenia var hotelRooms = _context.Rooms.Where(r => r.HotelId == id);
                _context.Rooms.RemoveRange(hotelRooms);

                //4. Remover los servicios adicionales que pertenecian al hotel que se quiere eliminar
                var hotelAdditionalServices = await _context.AdditionalServices
                    .Where(aS => aS.HotelId == id)
                    .ToListAsync();
                _context.AdditionalServices.RemoveRange(hotelAdditionalServices);

                //final: eliminar el hotel
                _context.Hotels.Remove(hotelEntity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseDto<HotelDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Registro borrado correctamente."
                };

            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Se produjo un error al eliminar el hotel.");
                return new ResponseDto<HotelDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo un error al eliminar el hotel."
                };
            }
        }
    }

    public async Task<ResponseDto<PaginationDto<List<HotelDto>>>> GetHotelsListForAdminPageAsync(
        string searchTerm = "", 
        int page = 1)
    {
        int startIndex = (page - 1) * PAGE_SIZE;

        // Filtro base que incluye todos los hoteles
        var hotelEntityQuery = _context.Hotels.AsQueryable();

        // Si searchTerm no está vacío, se agrega el filtro
        if (!string.IsNullOrEmpty(searchTerm))
        {
            hotelEntityQuery = hotelEntityQuery.Where(x =>
                (x.Name + " " + x.Description + " " + x.Overview + " " + x.Address)
                .ToLower().Contains(searchTerm.ToLower()));
        }

        int totalHotels = await hotelEntityQuery.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalHotels / PAGE_SIZE);

        // Paginación y ordenamiento
        var hotelsEntity = await hotelEntityQuery
            .OrderBy(x => x.Name)
            .Skip(startIndex)
            .Take(PAGE_SIZE)
            .ToListAsync();

        var hotelsDtos = _mapper.Map<List<HotelDto>>(hotelsEntity);

        return new ResponseDto<PaginationDto<List<HotelDto>>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Lista de registro obtenida correctamente.",
            Data = new PaginationDto<List<HotelDto>>
            {
                CurrentPage = page,
                PageSize = PAGE_SIZE,
                TotalItems = totalHotels,
                TotalPages = totalPages,
                Items = hotelsDtos,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages,
            }
        };
    }
}
