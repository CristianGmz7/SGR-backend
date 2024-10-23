using AutoMapper;
using Azure;
using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Hotels;
using GestionReservasHotelAPI.Dtos.Reservations;
using GestionReservasHotelAPI.Dtos.Rooms;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Services;

public class RoomsService : IRoomsService
{
    private readonly GestionReservasHotelContext _context;
    private readonly IMapper _mapper;
    private readonly int PAGE_SIZE;

    public RoomsService(
        GestionReservasHotelContext context, 
        IMapper mapper,
        IConfiguration configuration
        )
    {
        this._context = context;
        this._mapper = mapper;
        PAGE_SIZE = configuration.GetValue<int>("Pagination:RoomPageSize");
    }

    //Este servicio no se utiliza aun en el frontend
    //mapeo actualizado, espacios no necesarios no se cargan: addres, stars, numberPhone, overview
    public async Task<ResponseDto<List<RoomDto>>> GetRoomsListAsync()
    {
        var roomsEntity = await _context.Rooms
            .Include(room => room.Hotel)
            .ToListAsync();

        //el tipo sera List<RoomDto> y se convertira de RoomEntity que es la variable roomsEntity
        //verificar si para este caso es necesario descomentar el Id del Hotel del Dto
        //var roomsDto = _mapper.Map<List<RoomDto>>(roomsEntity);
        var roomsDto = roomsEntity.Select(room => new RoomDto
        {
            Id = room.Id,
            NumberRoom = room.NumberRoom,
            TypeRoom = room.TypeRoom,
            PriceNight = room.PriceNight,
            ImageUrl = room.ImageUrl,
            //este campo tiene virtual en el entity pero en el examen no
            HotelInfo = new HotelDto
            {
                //estos son los campos que se necesitan para el frontend
                Id = room.Hotel.Id,
                Name = room.Hotel.Name,
                Description = room.Hotel.Description,
                ImageUrl = room.Hotel.ImageUrl
            }
        }).ToList();

        return new ResponseDto<List<RoomDto>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Lista de registro obtenida correctamente.",
            Data = roomsDto
        };
    }

    //metodo obtener todas las habitaciones de un hotel, este se utiliza en el frontend
    //el id del hotel, el numero de pagina y las fechas de inicio y fin
    //aqui no se toma en cuenta la condicion porque se supone que habitaciones que se listan estaran disponibles segun las fechas
    public async Task<ResponseDto<PaginationDto<HotelDetailDto>>> GetRoomsOneHotelAsync(
        Guid id, int page = 1,
        DateTime filterStartDate = default, DateTime filterEndDate = default)
    {
        if (filterStartDate == default)
        {
            filterStartDate = DateTime.Now.AddMinutes(1);
        }
        if (filterEndDate == default)
        {
            filterEndDate = DateTime.Now.AddDays(1);
        }
        
        //posible solucion si llega a dar problemas, validar solo las fechas sin tomar en cuenta la hora
        if(filterStartDate < DateTime.Now)
        {
            return new ResponseDto<PaginationDto<HotelDetailDto>>
            {
                StatusCode = 404,
                Status = false,
                Message = "La fecha de inicio del filtro debe ser mayor que la fecha actual"
            };
        }
        
        if (filterEndDate < filterStartDate)
        {
            return new ResponseDto<PaginationDto<HotelDetailDto>>
            {
                StatusCode = 404,
                Status = false,
                Message = "La fecha de fin del filtro debe ser mayor que la fecha de inicio de filtro"
            };
        }
        var hotelEntity = await _context.Hotels.FindAsync(id);

        if(hotelEntity == null)
        {
            return new ResponseDto<PaginationDto<HotelDetailDto>>
            {
                StatusCode = 404,
                Status = false,
                Message = "El hotel no existe"
            };
        }
        
        int startIndex = (page - 1) * PAGE_SIZE;

        //AQUI TENGO UN DILEMA CON EL FRONTEND: El Query sera estatico o se ira actualizando
        //cuando se le filtrar y haya nuevo rango de fechas

        var roomEntityQuery = _context.Rooms
            .Include(x => x.Hotel)
            .Include(x => x.Reservations)
            .ThenInclude(x => x.Reservation)
            //verificar que la room sea del hotel y que no tenga reservas para las fechas ingresadas
            //verificar bien la parte del Where y !room.Reservation.Any
            .Where(room => room.HotelId == id && (
                !room.Reservations.Any(rr => 
                    rr.Reservation.StartDate < filterEndDate &&
                    rr.Reservation.FinishDate > filterStartDate)
            ));
        
        int totalRooms = await roomEntityQuery.CountAsync();

        int totalPages = (int)Math.Ceiling((double)totalRooms / PAGE_SIZE);

        var roomsEntity = await roomEntityQuery
            .OrderByDescending(x => x.NumberRoom)
            .Skip(startIndex)
            .Take(PAGE_SIZE)
            .ToListAsync();

        //EL MAPEO HACERLO MANUAL PORQUE SE DEBE INCLUIR CAMPO DEL NOMBRE DEL HOTEL Y DESCRIPCION
        //HACER EL MAPEO MANUAL POR EL CASO DE LAS HABITACIONES
        var roomsDto = roomsEntity.Select(room => new RoomDto
        {
            Id = room.Id,
            NumberRoom = room.NumberRoom,
            TypeRoom = room.TypeRoom,
            PriceNight = room.PriceNight,
            ImageUrl = room.ImageUrl
        }).ToList();

        var hotelDto = new HotelDto
        {
            Description = hotelEntity.Description,
            Address = hotelEntity.Address,
            Id = hotelEntity.Id,
            ImageUrl = hotelEntity.ImageUrl,
            Name = hotelEntity.Name,
            NumberPhone = hotelEntity.NumberPhone,
            Overview = hotelEntity.Overview,
            StarsMichelin = hotelEntity.StarsMichelin
        };

        var hotelDetail = new PaginationDto<HotelDetailDto>
        {
            CurrentPage = page,
            PageSize = PAGE_SIZE,
            TotalItems = totalRooms,
            TotalPages = totalPages,
            Items = new HotelDetailDto
            {
                Hotel = hotelDto,
                Rooms = roomsDto
            },
            HasPreviousPage = page > 1,
            HasNextPage = page < totalPages
        };

        return new ResponseDto<PaginationDto<HotelDetailDto>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registros encontrados correctamente",
            Data = hotelDetail
        };

    }

    public async Task<ResponseDto<RoomDto>> GetRoomById(Guid id)
    {
        var roomEntity = await _context.Rooms
            .Include(room => room.Hotel)
            .FirstOrDefaultAsync(r => r.Id == id);

        if(roomEntity == null)
        {
            return new ResponseDto<RoomDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "No se encontro el registro."
            };
        }

        var roomDto = new RoomDto
        {
            Id = roomEntity.Id,
            NumberRoom = roomEntity.NumberRoom,
            TypeRoom = roomEntity.TypeRoom,
            PriceNight = roomEntity.PriceNight,
            ImageUrl = roomEntity.ImageUrl,

            HotelInfo = new HotelDto
            {
                //estos son los campos que se necesitan para el frontend
                Id = roomEntity.Hotel.Id,
                Name = roomEntity.Hotel.Name,
                Description = roomEntity.Hotel.Description,
                ImageUrl = roomEntity.Hotel.ImageUrl
            }
        };

        return new ResponseDto<RoomDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registro encontrado exitosamente",
            Data = roomDto
        };
    }

    //no se coloco Includes porque al crear la habitacion se asigna con el hotel que se manda (supongo)
    public async Task<ResponseDto<RoomDto>> CreateAsync(RoomCreateDto dto)
    {
        var roomEntity = _mapper.Map<RoomEntity>(dto);

        var hotelEntity = await _context.Hotels.FindAsync(dto.HotelId);

        if (hotelEntity == null)
        {
            return new ResponseDto<RoomDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "El hotel no existe"
            };
        }

        var existingRoom = await _context.Rooms
            .Where(r => r.HotelId == dto.HotelId && r.NumberRoom == dto.NumberRoom)
            .FirstOrDefaultAsync();

        if(existingRoom != null) {
            return new ResponseDto<RoomDto>
            {
                StatusCode = 400,
                Status = false,
                Message = "El numero de habitacion ya existe en este hotel"
            };
        }

        _context.Rooms.Add(roomEntity);
        await _context.SaveChangesAsync();

        var roomDto = new RoomDto
        {
            Id = roomEntity.Id,
            NumberRoom = roomEntity.NumberRoom,
            TypeRoom = roomEntity.TypeRoom,
            PriceNight = roomEntity.PriceNight,
            ImageUrl = roomEntity.ImageUrl,

            HotelInfo = new HotelDto
            {
                //estos son los campos que se necesitan para el frontend
                Id = roomEntity.Hotel.Id,
                Name = roomEntity.Hotel.Name,
                Description = roomEntity.Hotel.Description,
                ImageUrl = roomEntity.Hotel.ImageUrl
            }
        };

        return new ResponseDto<RoomDto>
        {
            StatusCode = 201,
            Status = true,
            Message = "Registro creado correctamente",
            Data = roomDto
        };
    }

    public async Task<ResponseDto<RoomDto>> EditAsync (RoomEditDto dto, Guid id)
    {
        var roomEntity = await _context.Rooms
            .Include(room => room.Hotel)
            .FirstOrDefaultAsync(r => r.Id == id);

        if(roomEntity == null)
        {
            return new ResponseDto<RoomDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "No se encontro el registro"
            };
        }

        //Validar que no se repita numero de habitacion en el hotel donde pertenece
        var existingRoom = await _context.Rooms
             .Where(r => r.HotelId == roomEntity.HotelId && r.NumberRoom == dto.NumberRoom && r.Id != id)
             .FirstOrDefaultAsync();

        if (existingRoom != null)
        {
            return new ResponseDto<RoomDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "El número de habitación ya existe en este hotel"
            };
        }

        _mapper.Map<RoomEditDto, RoomEntity>(dto, roomEntity);

        _context.Rooms.Update(roomEntity);
        await _context.SaveChangesAsync();

        var roomDto = new RoomDto
        {
            Id = roomEntity.Id,
            NumberRoom = roomEntity.NumberRoom,
            TypeRoom = roomEntity.TypeRoom,
            PriceNight = roomEntity.PriceNight,
            ImageUrl = roomEntity.ImageUrl,

            HotelInfo = new HotelDto
            {
                //estos son los campos que se necesitan para el frontend
                Id = roomEntity.Hotel.Id,
                Name = roomEntity.Hotel.Name,
                Description = roomEntity.Hotel.Description,
                ImageUrl = roomEntity.Hotel.ImageUrl
            }
        };

        return new ResponseDto<RoomDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registro editado correctamente",
            Data = roomDto
        };

    }

    public async Task<ResponseDto<RoomDto>> DeleteAsync(Guid id)
    {
        var roomEntity = await _context.Rooms.FindAsync(id);

        if (roomEntity  == null)
        {
            return new ResponseDto<RoomDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "No se encontro el registro"
            };
        }

        _context.Rooms.Remove(roomEntity);
        await _context.SaveChangesAsync();

        return new ResponseDto<RoomDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Registro borrado existosamente"
        };
    }
}
