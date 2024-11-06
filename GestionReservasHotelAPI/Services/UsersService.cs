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

    public UsersService(
            GestionReservasHotelContext context,
            ILogger<UsersService> logger,
            UserManager<UserEntity> userManager
        )
    {
        this._context = context;
        this._logger = logger;
        this._userManager = userManager;
    }

    //metodo que se usará para cambiar el rol de un usuario a admin hotel o viceversa
    //esto no sera necesario ya se implementó en el HotelCreate y HotelDelete
    public async Task<ResponseDto<UserResponseDto>> ChangeRoleAsync (UserChangeRoleDto dto, string id)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var existRole = await _context.Roles.FindAsync(dto.RolId);

                if (existRole == null)
                {
                    return new ResponseDto<UserResponseDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "El rol no existe"
                    };
                }

                var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (userEntity == null)
                {
                    return new ResponseDto<UserResponseDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "No se encontró el usuario"
                    };
                }

                //Eliminar registro anterior del rol antes de crear el nuevo (esto debido al efecto cascada)
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userEntity.Id)
                    .ToListAsync();

                _context.UserRoles.RemoveRange(userRoles);

                //Agregar el nuevo rol que se manda
                var roleResult = await _userManager.AddToRoleAsync(userEntity, existRole.Name);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new ResponseDto<UserResponseDto>
                    {
                        StatusCode = 500,
                        Status = false,
                        Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                    };
                }

                //antes de retornar la respuesta cerrar transaccion con un commit
                //throw new Exception("Error para probar el Rollback.");
                await transaction.CommitAsync();

                var userDto = new UserResponseDto
                {
                    Id = userEntity.Id,
                    FirstName = userEntity.FirstName,
                    LastName = userEntity.LastName,
                    Email = userEntity.Email,
                    //RolId = dto.RolId,        //funciona con cualquiera de los dos
                    RolId = existRole.Id
                };

                return new ResponseDto<UserResponseDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Usuario editado satisfactoriamente",
                    Data = userDto,
                };
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Se produjo un error al cambiar el rol del usuario.");
                return new ResponseDto<UserResponseDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo un error al cambiar el rol del usuario."
                };
            }
        }
    }
}
