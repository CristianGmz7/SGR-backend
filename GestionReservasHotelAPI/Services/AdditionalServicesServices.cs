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

    public AdditionalServicesServices(GestionReservasHotelContext context, IMapper mapper)
    {
        this._context = context;
        this._mapper = mapper;
    }

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

    public async Task<ResponseDto<AdditionalServiceDto>> DeleteAsync(Guid id)
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

        _context.AdditionalServices.Remove(additionalServiceEntity);
        await _context.SaveChangesAsync();

        return new ResponseDto<AdditionalServiceDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registro borrado existosamente"
        };
    }
}
