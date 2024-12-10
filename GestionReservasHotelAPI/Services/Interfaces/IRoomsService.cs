﻿using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Hotels;
using GestionReservasHotelAPI.Dtos.Rooms;

namespace GestionReservasHotelAPI.Services.Interfaces;

public interface IRoomsService
{
    Task<ResponseDto<RoomDto>> CreateAsync(RoomCreateDto dto);
    Task<ResponseDto<RoomDto>> DeleteAsync(Guid id);
    Task<ResponseDto<RoomDto>> EditAsync(RoomEditDto dto, Guid id);
    Task<ResponseDto<RoomDto>> GetRoomById(Guid id);
    Task<ResponseDto<PaginationDto<HotelDetailDto>>> GetRoomsByAdminHotelAsync(Guid id, string searchTerm = "", int page = 1);
    Task<ResponseDto<List<RoomDto>>> GetRoomsListAsync();
    Task<ResponseDto<PaginationDto<HotelDetailDto>>> GetRoomsOneHotelAsync(Guid id, int page = 1, DateTime filterStartDate = default, 
        DateTime filterEndDate = default, double priceMin = -1, double priceMax = -1, string type = "");
}
