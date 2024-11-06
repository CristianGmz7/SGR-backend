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
        int page = 1)
    {
        int startIndex = (page - 1) * PAGE_SIZE;

        //validacion falsa para que todso los hoteles los pase al Query
        var hotelEntityQuery = _context.Hotels
            .Where(x => x.Id == x.Id);

        int totalHotels = await hotelEntityQuery.CountAsync();

        int totalPages = (int)Math.Ceiling((double)totalHotels / PAGE_SIZE);

        //¿Que factor usar de filtro como ordenamiento?
        var hotelsEntity = await _context.Hotels
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

        var hotelDto = _mapper.Map<HotelDto>(hotelEntity);

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

                string adminHotelId = "00d3f5ce-36e7-4f60-a7e8-b791f7628da5";
                var adminHotelRole = await _context.Roles.FindAsync(adminHotelId);

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
                string userId = "0416b6e2-509c-42e0-acf3-a1157e623d9b";
                var userRole = await _context.Roles.FindAsync(userId);

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

                //final
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
}
