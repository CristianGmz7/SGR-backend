using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Reactions;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Services;

public class HotelsReactsService : IHotelsReactsService
{
    private readonly GestionReservasHotelContext _context;
    private readonly ILogger<HotelsReactsService> _logger;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IAuditService _auditService;

    public HotelsReactsService(
            GestionReservasHotelContext context,
            ILogger<HotelsReactsService> logger,
            UserManager<UserEntity> userManager,
            IAuditService auditService
        )
    {
        this._context = context;
        this._logger = logger;
        this._userManager = userManager;
        this._auditService = auditService;
    }

    public async Task<ResponseDto<HotelReactResponseDto>> GetByHotelAndUserAsync (Guid HotelId, string isAuthenticated)
    {
        var hotelEntity = await _context.Hotels.FindAsync(HotelId);
        if (hotelEntity == null)
        {
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "El hotel no existe"
            };
        }

        var hotelReactDto = new HotelReactResponseDto();
        //si no esta autenticado no hacer nada
        if (isAuthenticated == "NOISAUTHENTICATED")
        {
            hotelReactDto.Action = "NOUSER";
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = true,
                StatusCode = 200,
                Message = "Inicia sesión para poder reaccionar",
                Data = hotelReactDto
            };
        }
        var userId = _auditService.GetUserId();

        //si existe una reaccion determinar si es LIKED O UNLIKED
        var existsReact = await _context.HotelsReacts.FirstOrDefaultAsync(x => x.HotelId == hotelEntity.Id && x.UserId == userId);
        if (existsReact != null)
        {
            hotelReactDto.Id = existsReact.Id;
            hotelReactDto.HotelId = existsReact.HotelId;
            hotelReactDto.UserId = existsReact.UserId;
            hotelReactDto.Reaction = existsReact.Reaction;
            hotelReactDto.Action = existsReact.Reaction ? "LIKED" : "UNLIKED";

            return new ResponseDto<HotelReactResponseDto>
            {
                Status = true,
                StatusCode = 200,
                Message = "Reacción obtenida satisfactoriamente",
                Data = hotelReactDto
            };
        }

        //si no existe reaccion entonces NONE
        hotelReactDto.Action = "NONE";
        return new ResponseDto<HotelReactResponseDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "No existe reacción",
            Data = hotelReactDto
        };
    }

    public async Task<ResponseDto<HotelReactResponseDto>> CreateAsync (HotelReactCreateDto dto)
    {
        var hotelEntity = await _context.Hotels.FindAsync(dto.HotelId);

        if (hotelEntity == null)
        {
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "El hotel no existe"
            };
        }

        var userId = _auditService.GetUserId();

        var existsReact = await _context.HotelsReacts.FirstOrDefaultAsync(x => x.HotelId == hotelEntity.Id && x.UserId == userId);
        if(existsReact != null)
        {
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "Ya existe una reacción no es posible crear otra"
            };
        }

        var reactValue = dto.Action == "LIKED" ? true : false;
        var hotelReactEntity = new HotelReactEntity
        {
            HotelId = dto.HotelId,
            UserId = userId,
            Reaction = reactValue
        };

        //throw new Exception("Error para verificar que se estaba creando correctamente");
        _context.HotelsReacts.Add( hotelReactEntity );
        await _context.SaveChangesAsync();

        var hotelReactDto = new HotelReactResponseDto
        {
            Id = hotelReactEntity.Id,
            HotelId = hotelReactEntity.HotelId,
            UserId = hotelReactEntity.UserId,
            Reaction = hotelReactEntity.Reaction,
            Action = dto.Action
        };

        return new ResponseDto<HotelReactResponseDto>
        {
            Status = true,
            StatusCode = 201,
            Message = "Reacción creada satisfactoriamente",
            Data = hotelReactDto
        };
    }

    public async Task<ResponseDto<HotelReactResponseDto>> EditAsync (HotelReactEditDto dto)
    {
        var hotelEntity = await _context.Hotels.FindAsync(dto.HotelId);

        if (hotelEntity == null)
        {
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "El hotel no existe"
            };
        }

        var userId = _auditService.GetUserId();
        var hotelReactEntity = await _context.HotelsReacts.FirstOrDefaultAsync(x => x.HotelId == hotelEntity.Id && x.UserId == userId);
        if (hotelReactEntity == null)
        {
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "El registro de la reacción no existe"
            };
        }
        if(dto.Action != "SWITCHLIKED")
        {
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "Acción invalida para actualizar registro"
            };
        }

        hotelReactEntity.Reaction = !hotelReactEntity.Reaction;

        //throw new Exception("Error para verificar que se estaba creando correctamente");
        _context.HotelsReacts.Update(hotelReactEntity);
        await _context.SaveChangesAsync();

        var hotelReactDto = new HotelReactResponseDto
        {
            Id = hotelReactEntity.Id,
            HotelId = hotelReactEntity.HotelId,
            UserId = hotelReactEntity.UserId,
            Reaction = hotelReactEntity.Reaction,
            Action = hotelReactEntity.Reaction ? "LIKED" : "UNLIKED"
        };

        return new ResponseDto<HotelReactResponseDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Reacción editada satisfactoriamente",
            Data = hotelReactDto
        };
    }

    public async Task<ResponseDto<HotelReactResponseDto>> DeleteAsync (Guid HotelId, string Action)
    {
        var hotelEntity = await _context.Hotels.FindAsync(HotelId);

        if (hotelEntity == null)
        {
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "El hotel no existe"
            };
        }

        var userId = _auditService.GetUserId();
        var hotelReactEntity = await _context.HotelsReacts.FirstOrDefaultAsync(x => x.HotelId == hotelEntity.Id && x.UserId == userId);
        if (hotelReactEntity == null)
        {
            return new ResponseDto<HotelReactResponseDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "El registro de la reacción no existe",

            };
        }

        //throw new Exception("Error para verificar que se estaba creando correctamente");
        _context.HotelsReacts.Remove(hotelReactEntity);
        await _context.SaveChangesAsync();

        var hotelReactDto = new HotelReactResponseDto
        {
            Action = Action
        };

        return new ResponseDto<HotelReactResponseDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Reacción eliminada satisfactoriamente",
            Data = hotelReactDto
        };

    }
    //se van a crear 4 metodos: obtener por id de hotel e id de usuario logueado, crear, editar y eliminar
    //el obtener es cuando se entra a la pagina HotelRoomList ahi debe mostrarse, de ser posible anexar respuesta mejor en el HotelDetailInfo  (YA IMPLEMENTANDO BACKEND)
    //el de crear es cuando no exista ningun registro de like o dislike (YA IMPLEMENTANDO BACKEND)
    //el de editar es cuando existe like o dislike y se cambio de reaccion (YA IMPLEMENTANDO BACKEND)
    //el de eliminar es cuando existe like o dislike y se vuelve a presionar (YA IMPLEMENTANDO BACKEND)
}
