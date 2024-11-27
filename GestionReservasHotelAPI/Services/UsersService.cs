using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Dtos.Users;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Services;

public class UsersService : IUsersService
{
    private readonly GestionReservasHotelContext _context;
    private readonly ILogger _logger;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IAuditService _auditService;

    public UsersService(
            GestionReservasHotelContext context,
            ILogger<UsersService> logger,
            UserManager<UserEntity> userManager,
            IAuditService auditService
        )
    {
        this._context = context;
        this._logger = logger;
        this._userManager = userManager;
        this._auditService = auditService;
    }

    //este metodo se usará para crear reservaciones a responsabilidad de ese usuario
    public async Task<ResponseDto<List<BasicUserInformationResponseDto>>> GetUserListAsync()
    {
        // Obtén el ID del usuario actualmente logueado
        string currentUserId = _auditService.GetUserId();

        // Consulta la base de datos para obtener todos los usuarios excepto el logueado
        var usersEntity = await _context.Users
            .Where(u => u.Id != currentUserId) // Filtrar el usuario logueado
            .ToListAsync();

        // Mapea los usuarios a DTOs
        var users = usersEntity.Select(u => new BasicUserInformationResponseDto
        {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            Email = u.Email,
            ProfilePictureUrl = u.ProfilePictureUrl
        }).ToList();

        // Retorna la respuesta
        return new ResponseDto<List<BasicUserInformationResponseDto>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Lista de usuarios obtenida correctamente",
            Data = users
        };
    }

    //si queda chance averiguar acerca de la paginacion en el metodo anterior para que se use dentro de un select

    //este metodo se usará para crear hoteles y asignar el role de HOTELADMIN
    public async Task<ResponseDto<List<BasicUserInformationResponseDto>>> GetUsersListWithUserRoleAsync(string searchTerm = "")
    {
        string currentUserId = _auditService.GetUserId();

        //var usersEntity = await _context.Users
        //    .Where(u => u.Id != currentUserId &&
        //                u.UserRoles.Any(ur => _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == RolesConstant.USER)))
        //    .ToListAsync();
        var userQuery = _context.Users
            .Where(u => u.Id != currentUserId &&
                u.UserRoles.Any(ur => _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == RolesConstant.USER)));

        if (!string.IsNullOrEmpty(searchTerm))
        {
            userQuery = userQuery
                .Where(x => (x.FirstName + " " + x.LastName + " " + x.Id)
                .ToLower().Contains(searchTerm.ToLower()));
        }

        var usersEntity = await userQuery.ToListAsync();

        var usersDto = usersEntity.Select(u => new BasicUserInformationResponseDto
        {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            Email = u.Email,
            ProfilePictureUrl = u.ProfilePictureUrl
        }).ToList();

        return new ResponseDto<List<BasicUserInformationResponseDto>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Lista de usuarios con rol USER obtenida correctamente",
            Data = usersDto
        };
    }
}
