using GestionReservasHotelAPI.Dtos.AdditionalServices;
using GestionReservasHotelAPI.Dtos.Common;

namespace GestionReservasHotelAPI.Services.Interfaces;

public interface IAdditionalServicesServices
{
    Task<ResponseDto<AdditionalServiceDto>> CreateAsync(AdditionalServiceCreateDto dto);
    Task<ResponseDto<AdditionalServiceDto>> DeleteAsync(Guid id);
    Task<ResponseDto<AdditionalServiceDto>> EditAsync(AdditionalServiceEditDto dto, Guid id);
    Task<ResponseDto<AdditionalServiceDto>> GetAdditionalServiceById(Guid id);
    Task<ResponseDto<List<AdditionalServiceDto>>> GetAdditionalServicesAsync();
    Task<ResponseDto<List<AdditionalServiceDto>>> GetAdditionalServicesOneHotelAsync(Guid id);
}
