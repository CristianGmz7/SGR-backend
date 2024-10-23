using AutoMapper;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.AdditionalServices;
using GestionReservasHotelAPI.Dtos.Hotels;
using GestionReservasHotelAPI.Dtos.Reservations;
using GestionReservasHotelAPI.Dtos.Rooms;
//using GestionReservasHotelAPI.Dtos.Reservations;

namespace GestionReservasHotelAPI.Helpers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        MapsForHotels();
        MapsForRooms();
        MapsForAdditionalServices();
    }

    private void MapsForHotels()
    {
        CreateMap<HotelEntity, HotelDto>();
        CreateMap<HotelCreateDto, HotelEntity>();
        CreateMap<HotelEditDto, HotelEntity>();
    }
    
    private void MapsForRooms()
    {
        //src: Entity; dest: Dto; convertir de Entity a Dto (Metodos Get)
        CreateMap<RoomEntity, RoomDto>();

        //src: Dto; dest: Entity; convertir CreateDto y EditDto a Entity (Metodos Post y Put)
        CreateMap<RoomCreateDto, RoomEntity>();
        CreateMap<RoomEditDto, RoomEntity>();
    }

    private void MapsForAdditionalServices()
    {
        CreateMap<AdditionalServiceEntity, AdditionalServiceDto>();
        CreateMap<AdditionalServiceCreateDto, AdditionalServiceEntity>();
        CreateMap<AdditionalServiceEditDto, AdditionalServiceEntity>();
    }

    private void MapsForReservation()
    {
        //esto creo que no lo utilzare
        CreateMap<ReservationEntity, ReservationDto>();
        CreateMap<ReservationCreateDto, ReservationEntity>();
    }
}
