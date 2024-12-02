using AutoMapper;
using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.AdditionalServices;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Rooms;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Services;

public class AdditionalServicesServices : IAdditionalServicesServices
{
    private readonly GestionReservasHotelContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AdditionalServicesServices> _logger;
    private readonly IAuditService _auditService;

    public AdditionalServicesServices(
            GestionReservasHotelContext context, 
            IMapper mapper,
            ILogger<AdditionalServicesServices> logger,
            IAuditService auditService 
        )
    {
        this._context = context;
        this._mapper = mapper;
        this._logger = logger;
        this._auditService = auditService;
    }

    //este servicio se coloco en el servicio que solo es permitido por rol PAGEADMIN
    public async Task<ResponseDto<List<AdditionalServiceDto>>> GetAdditionalServicesAsync()
    {
        var additionalServicesEntity = await _context.AdditionalServices.ToListAsync();

        var additionalServicesDto = _mapper.Map<List<AdditionalServiceDto>>(additionalServicesEntity);

        return new ResponseDto<List<AdditionalServiceDto>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Lista de registros obtenida correctamente",
            Data = additionalServicesDto
        };
    }

    // no creo que sea necesario colocarle paginación
    public async Task <ResponseDto<List<AdditionalServiceDto>>> GetAdditionalServicesOneHotelAsync(Guid id)
    {
        var hotelEntity = await _context.Hotels.FindAsync(id);

        if (hotelEntity == null)
        {
            return new ResponseDto<List<AdditionalServiceDto>>
            {
                StatusCode =  404,
                Status = false,
                Message = "El hotel no existe"
            };
        }

        var additionalServicesEntity = await _context.AdditionalServices.ToListAsync();
        var additionalServicesOfHotel = new List<AdditionalServiceEntity> { };

        foreach (var additServ in additionalServicesEntity)
        {
            if(additServ.HotelId == hotelEntity.Id)
            {
                additionalServicesOfHotel.Add(additServ);
            }
            
        }

        var additioonalServicesDto = _mapper.Map<List<AdditionalServiceDto>>(additionalServicesOfHotel);

        return new ResponseDto<List<AdditionalServiceDto>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registros encontrados correctamente",
            Data = additioonalServicesDto
        };
    }

    // para editar servicio adicional se usaria este servicio para el frontend
    public async Task<ResponseDto<AdditionalServiceDto>> GetAdditionalServiceById(Guid id)
    {
        var additionalServiceEntity = await _context.AdditionalServices.FindAsync(id);

        if(additionalServiceEntity == null)
        {
            return new ResponseDto<AdditionalServiceDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "No se encontro el registro."
            };
        }

        var additionalServiceDto = _mapper.Map<AdditionalServiceDto>(additionalServiceEntity);

        return new ResponseDto<AdditionalServiceDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registro encontrado exitosamente",
            Data = additionalServiceDto
        };
    }

    //nuevas implementaciones de users copiadas de creacion habitaciones
    public async Task<ResponseDto<AdditionalServiceDto>> CreateAsync(AdditionalServiceCreateDto dto)
    {
        var hotelEntity = await _context.Hotels.FindAsync(dto.HotelId);

        if (hotelEntity == null)
        {
            return new ResponseDto<AdditionalServiceDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "El hotel no existe"
            };
        }

        //verificar que el que quiera crear un servicio adicional sea el administrador del hotel del SA
        var userId = _auditService.GetUserId();
        if (userId != hotelEntity.AdminUserId)
        {
            return new ResponseDto<AdditionalServiceDto>
            {
                StatusCode = 400,
                Status = false,
                Message = "Solo el administrador del hotel puede crear servicios adicionales"
            };
        }

        var existingAdditionalService = await _context.AdditionalServices
            .Where(aS => aS.HotelId == dto.HotelId && aS.Name.Trim().ToLower() == dto.Name.Trim().ToLower())
            .FirstOrDefaultAsync();

        if(existingAdditionalService != null)
        {
            return new ResponseDto<AdditionalServiceDto>
            {
                StatusCode = 400,
                Status = false,
                Message = "El nombre de servicio adicional ya existe en este hotel"
            };
        }

        var additionalServiceEntity = _mapper.Map<AdditionalServiceEntity>(dto);

        _context.AdditionalServices.Add(additionalServiceEntity);
        await _context.SaveChangesAsync();

        var additionalServiceDto = _mapper.Map<AdditionalServiceDto>(additionalServiceEntity);

        return new ResponseDto<AdditionalServiceDto>
        {
            StatusCode = 201,
            Status = true,
            Message = "Registro creado correctamente",
            Data = additionalServiceDto
        };
    }

    //AUN FALTA PROBAR ESTE SERVICIO Y AGREGARLO AL CONTROLADOR Y AL BRUNO
    //nuevas implementaciones de users copiadas de editar habitaciones
    public async Task<ResponseDto<AdditionalServiceDto>> EditAsync(AdditionalServiceEditDto dto, Guid id)
    {
        var additionalServiceEntity = await _context.AdditionalServices.FindAsync(id);

        if(additionalServiceEntity == null)
        {
            return new ResponseDto<AdditionalServiceDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "No se encontro el registro"
            };
        }

        var hotelEntity = await _context.Hotels.FindAsync(additionalServiceEntity.HotelId);
        if (hotelEntity == null)
        {
            return new ResponseDto<AdditionalServiceDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "El hotel no existe"
            };
        }

        //verificar que el que quiera editar una habitacion sea el administrador del hotel de la habitacion
        var userId = _auditService.GetUserId();
        if (userId != hotelEntity.AdminUserId)
        {
            return new ResponseDto<AdditionalServiceDto>
            {
                StatusCode = 400,
                Status = false,
                Message = "Solo el administrador del hotel puede editar sus servicios adicionales"
            };
        }

        var existingAdditionalService = await _context.AdditionalServices
            .Where(aS => aS.HotelId == additionalServiceEntity.HotelId
            && aS.Name.Trim().ToLower() == dto.Name.Trim().ToLower()
            && aS.Id != id)
            .FirstOrDefaultAsync();

        if(existingAdditionalService != null)
        {
            return new ResponseDto<AdditionalServiceDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "El nombre de servicio adicional ya existe en este hotel"
            };
        }

        _mapper.Map<AdditionalServiceEditDto, AdditionalServiceEntity>(dto, additionalServiceEntity);

        _context.AdditionalServices.Update(additionalServiceEntity);
        await _context.SaveChangesAsync();

        var additionalServiceDto = _mapper.Map<AdditionalServiceDto>(additionalServiceEntity);

        return new ResponseDto<AdditionalServiceDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registro editado correctamente",
            Data = additionalServiceDto
        };

    }

    //nuevas implementaciones de users copiadas de borrar habitaciones
    public async Task<ResponseDto<AdditionalServiceDto>> DeleteAsync(Guid id)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var additionalServiceEntity = await _context.AdditionalServices.FindAsync(id);

                if (additionalServiceEntity == null)
                {
                    return new ResponseDto<AdditionalServiceDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "No se encontro el registro"
                    };
                }

                //hotel del SA que se quiera eliminar
                var hotelEntity = await _context.Hotels.FindAsync(additionalServiceEntity.HotelId);
                if (hotelEntity == null)
                {
                    return new ResponseDto<AdditionalServiceDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "El hotel no existe"
                    };
                }

                var userId = _auditService.GetUserId();
                if (userId != hotelEntity.AdminUserId)
                {
                    return new ResponseDto<AdditionalServiceDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "Solo el administrador del hotel puede eliminar sus servicios adicionales"
                    };
                }

                bool hasActiveReservations = await _context.Reservations
                    .AnyAsync(res => res.AdditionalServices.Any(asr => asr.AdditionalServiceId == id) && res.FinishDate > DateTime.Now);

                if (hasActiveReservations)
                {
                    return new ResponseDto<AdditionalServiceDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = "No se puede eliminar el servicio adicional porque se encuentra en reservas activas"
                    };
                }

                var additionalServicesReservations = await _context.AdditionalServiceReservations
                    .Where(asr => asr.AdditionalServiceId == id)
                    .ToListAsync();
                _context.AdditionalServiceReservations.RemoveRange(additionalServicesReservations);

                //IMPORTANTE: ELIMINADA EL SERVICIO ADICIONAL AL MOMENTO DE CUANDO SE MUESTRE LA LISTA DE RESERVACIONES DONDE HAYA ESTADO EL SERVICIO ADICIONAL ELIMINADO
                //  PUEDE DAR PROBLEMAS DE CONGRUENCIA EN LOS DATOS, SE TIENE QUE PROBAR ESTE ENDPOINT A VER COMO RESPONDE EL FRONTEND ANTE LA AUSENCIA DE UN SERVICIO ADICIONAL
                //      PUEDE OCURRIR DE MANERA SIMILAR CUANDO SE ELIMINE UNA HABITACION

                //eliminar servicio adicional
                _context.AdditionalServices.Remove(additionalServiceEntity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResponseDto<AdditionalServiceDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Registro borrado existosamente"
                };
            }
            catch(Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Error al borrar el servicio adicional");

                return new ResponseDto<AdditionalServiceDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Error al borrar el servicio adicional"
                };
            }
        }
    }
}
