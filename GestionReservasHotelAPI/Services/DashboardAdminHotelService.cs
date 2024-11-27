using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Dashboards.DashboardAdminHotel;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Services;

public class DashboardAdminHotelService : IDashboardAdminHotelService
{
    private readonly GestionReservasHotelContext _context;
    private readonly ILogger<DashboardAdminHotelService> _logger;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;

    public DashboardAdminHotelService(
            GestionReservasHotelContext context,
            ILogger<DashboardAdminHotelService> logger,
            IAuditService auditService,
            IConfiguration configuration
        )
    {
        this._context = context;
        this._logger = logger;
        this._auditService = auditService;
        this._configuration = configuration;
    }

    public async Task<ResponseDto<DashboardHotelIdResponseDto>> GetHotelIdAsync()
    {
        // Obtén el ID del usuario actualmente logueado
        string currentUserId = _auditService.GetUserId();

        // Obtén el hotel donde el AdminUserId coincide con el usuario logueado
        var hotelEntity = await _context.Hotels
            .Where(h => h.AdminUserId == currentUserId)
            .FirstOrDefaultAsync();

        // Verifica si se encontró un hotel
        if (hotelEntity == null)
        {
            _logger.LogWarning($"No se encontró ningún hotel administrado por el usuario con ID {currentUserId}");
            return new ResponseDto<DashboardHotelIdResponseDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "No se encontró ningún hotel para el usuario actual",
            };
        }

        // Mapea la entidad al DTO
        var hotelDto = new DashboardHotelIdResponseDto
        {
            Id = hotelEntity.Id
        };

        // Retorna la respuesta con el DTO
        return new ResponseDto<DashboardHotelIdResponseDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Operación realizada correctamente",
            Data = hotelDto
        };
    }


}
