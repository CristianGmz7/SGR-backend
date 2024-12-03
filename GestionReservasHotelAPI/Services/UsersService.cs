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
    public async Task<ResponseDto<List<BasicUserInformationResponseDto>>> GetUserListAsync(string searchTerm = "")
    {
        // Obtén el ID del usuario actualmente logueado
        string currentUserId = _auditService.GetUserId();

        //obtener todos los usuarios registrados en la base de datos
        var userQuery = _context.Users
            .Where(u => u.Id != currentUserId);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            userQuery = userQuery
                .Where(x => (x.FirstName + " " + x.LastName + " " + x.Id)
                .ToLower().Contains(searchTerm.ToLower()));
        }

        var usersEntity = await userQuery.ToListAsync();


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


    //metodo para obtener data publica de un usuario cuando quiera cambiar sus datos
    public async Task<ResponseDto<UserLoggedResponseDto>> GetUserInfoLoggedAsync ()
    {
        string currentUserId = _auditService.GetUserId();
        var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (userEntity is null)
        {
            return new ResponseDto<UserLoggedResponseDto>
            {
                StatusCode = 404,
                Status = false,
                Message = "El usuario no existe"
            };
        }

        var userDto = new UserLoggedResponseDto
        {
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            ProfilePictureUrl= userEntity.ProfilePictureUrl
        };


        return new ResponseDto<UserLoggedResponseDto>
        {
            StatusCode = 200,
            Status = true,
            Message = "Informacion usuario logueado cargada correctamente",
            Data = userDto
        };
    }

    //falta que probar en bruno con rollback, tiene que copiarse info del jwtio asi como campos de passwordhashed para verificar cambios de antes y despues
    //cambiar datos, quitar token y volver a generarlo para verificar campos del token
    //EN EL FRONTEND despues de ejecutar este metodo se cerrará sesion
    public async Task<ResponseDto<BasicUserInformationResponseDto>> EditUserAsync(UserEditDto dto)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                string currentUserId = _auditService.GetUserId();
                var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);

                if (userEntity is null)
                {
                    return new ResponseDto<BasicUserInformationResponseDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "El usuario no existe"
                    };
                }

                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                {
                    userEntity.FirstName = dto.FirstName;
                }
                if (!string.IsNullOrWhiteSpace(dto.LastName))
                {
                    userEntity.LastName = dto.LastName;
                }
                if (!string.IsNullOrWhiteSpace(dto.ProfilePictureUrl))
                {
                    userEntity.ProfilePictureUrl = dto.ProfilePictureUrl;
                }

                //verificar que el correo electronico no se repita 
                if (!string.IsNullOrWhiteSpace(dto.NewEmail) && userEntity.Email != dto.NewEmail)  {

                    //verificar que el correo antiguo sea el verdadero del usuario
                    if(dto.OldEmail != userEntity.Email)
                    {
                        return new ResponseDto<BasicUserInformationResponseDto>
                        {
                            Status = false,
                            StatusCode = 400,
                            Message = "El correo actual ingresado no es el correcto"
                        };
                    }

                    //verificar si el nuevo correo ya existe
                    var emailExists = await _userManager.FindByEmailAsync(dto.NewEmail);
                    if(emailExists != null)
                    {
                        return new ResponseDto<BasicUserInformationResponseDto>
                        {
                            StatusCode = 400,
                            Status = false,
                            Message = "El correo nuevo ingresado ya está en uso"
                        };
                    }
                    //si no esta en uso cambiar el username y email
                    userEntity.Email = dto.NewEmail;
                    userEntity.UserName = dto.NewEmail;
                }

                //verificar que la contraseña sea la misma antes de hacer el mapeo
                if(!string.IsNullOrWhiteSpace(dto.NewPassword) && !string.IsNullOrWhiteSpace(dto.OldPassword))
                {
                    var passwordCheck = await _userManager.CheckPasswordAsync(userEntity, dto.OldPassword);
                    if (!passwordCheck)
                    {
                        return new ResponseDto<BasicUserInformationResponseDto>
                        {
                            StatusCode = 400,
                            Status = false,
                            Message = "La contraseña actual es incorrecta"
                        };
                    }

                    var changePasswordResult = await _userManager.ChangePasswordAsync(userEntity, dto.OldPassword, dto.NewPassword);
                    if (!changePasswordResult.Succeeded)
                    {
                        return new ResponseDto<BasicUserInformationResponseDto>
                        {
                            StatusCode = 400,
                            Status = false,
                            Message = "No se pudo actualizar la contraseña",
                        };
                    }


                }

                var result = await _userManager.UpdateAsync(userEntity);
                if (!result.Succeeded)
                {
                    return new ResponseDto<BasicUserInformationResponseDto>
                    {
                        StatusCode = 500,
                        Status = false,
                        Message = "No se pudo actualizar la informacion del usuario"
                    };
                }

                //probar rollback
                //throw new Exception("Error para probar el rollback");
                await transaction.CommitAsync();

                var userDto = new BasicUserInformationResponseDto
                {
                    Id = userEntity.Id,
                    FullName = userEntity.FirstName + " " + userEntity.LastName,
                    Email = userEntity.Email,
                    ProfilePictureUrl = userEntity.ProfilePictureUrl,
                };

                return new ResponseDto<BasicUserInformationResponseDto>
                {
                    Status = true,
                    StatusCode = 200,
                    Message = "Informacion actualizada correctamete",
                    Data = userDto
                };
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e, "Se produjo un error al editar el usuario.");
                return new ResponseDto<BasicUserInformationResponseDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo un error al editar el usuario."
                };
            }
        }
    }

    //crear metodo que retorne un booleano, en el que se reciba la contraseña y se verifique que es la contraseña correcta, esto será implementando cuando se quiera cambiar contraseña o correo electronico
    public async Task<ResponseDto<bool>> ConfirmPasswordUserToEditAsync (string password)
    {

        if (string.IsNullOrWhiteSpace(password))
        {
            return new ResponseDto<bool>
            {
                StatusCode = 400,
                Status = false,
                Message = "Debe ingresar una contraseña",
                Data = false
            };
        }

        string currentUserId = _auditService.GetUserId();
        var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (userEntity is null)
        {
            return new ResponseDto<bool>
            {
                StatusCode = 404,
                Status = false,
                Message = "El usuario no existe",
                Data = false
            };
        }

        var passwordCheck = await _userManager.CheckPasswordAsync(userEntity, password);
        if (!passwordCheck)
        {
            return new ResponseDto<bool>
            {
                StatusCode = 400,
                Status = false,
                Message = "La contraseña es incorrecta",
                Data = false
            };
        }


        return new ResponseDto<bool>
        {
            Status = true,
            StatusCode = 200,
            Message = "La contraseña es valida",
            Data = true
        };
    }
}
