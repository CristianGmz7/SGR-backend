﻿using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Hotels;

namespace GestionReservasHotelAPI.Services.Interfaces
{
    public interface IHotelsService
    {
        Task<ResponseDto<PaginationDto<List<HotelDto>>>> GetHotelsListForAdminPageAsync(string searchTerm = "", int page = 1);
        Task<ResponseDto<HotelDto>> GetHotelByIdAsync(Guid id);
        Task<ResponseDto<HotelDto>> CreateAsync(HotelCreateDto dto);
        Task<ResponseDto<HotelDto>> EditAsync(HotelEditDto dto, Guid id);
        Task<ResponseDto<HotelDto>> DeleteAsync(Guid id);
        Task<ResponseDto<PaginationDto<List<HotelDto>>>> GetHotelsListAsync(string searchTerm = "", int page = 1, int starsNumber = 0, 
            string department = "", string city = "", int minLikes = -1, int maxLikes = -1);
    }
}
