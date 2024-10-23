using AutoMapper;
using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Hotels;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Services
{
    public class HotelsService : IHotelsService
    {
        private readonly GestionReservasHotelContext _context;
        private readonly IMapper _mapper;
        private readonly int PAGE_SIZE;

        public HotelsService(GestionReservasHotelContext context, 
            IMapper mapper,
            IConfiguration configuration) 
        {
            this._context = context;
            this._mapper = mapper;
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
            var hotelEntity = _mapper.Map<HotelEntity>(dto);

            // TODO: Validar que el hotel no se repita.

            _context.Hotels.Add(hotelEntity);

            await _context.SaveChangesAsync();

            var hotelDto = _mapper.Map<HotelDto>(hotelEntity);

            return new ResponseDto<HotelDto>
            {
                StatusCode = 201,
                Status = true,
                Message = "Resgistro creado exitosamente",
                Data = hotelDto
            };
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
            var hotelEntity = await _context.Hotels.FirstOrDefaultAsync(x =>  x.Id == id);
            if (hotelEntity == null) 
            {
                return new ResponseDto<HotelDto>
                {
                    StatusCode = 404,
                    Status = false,
                    Message = "No se encontro el registro."
                };
            }

            _context.Hotels.Remove(hotelEntity);
            await _context.SaveChangesAsync();

            return new ResponseDto<HotelDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Registro borrado correctamente."
            };
        }
    }
}
