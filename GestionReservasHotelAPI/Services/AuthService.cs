using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.Auth;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestionReservasHotelAPI.Services;

public class AuthService : IAuthService
{
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            IConfiguration configuration
        )
    {
        this._signInManager = signInManager;
        this._userManager = userManager;
        this._configuration = configuration;
    }

    public async Task<ResponseDto<LoginResponseDto>> LoginAsync (LoginDto dto)
    {
        var result = await _signInManager
            .PasswordSignInAsync(dto.Email,
            dto.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var userEntity = await _userManager.FindByEmailAsync(dto.Email);

            var authClaims = new List<Claim>
            {
                //esto no esta hasheado se manda como un base64
                new Claim(ClaimTypes.Email, userEntity.Email),  //email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  //guid unico
                new Claim("UserId", userEntity.Id)  //id 
            };

            var userRoles = await _userManager.GetRolesAsync(userEntity);

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtToken = GetToken(authClaims);

            return new ResponseDto<LoginResponseDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Inicio de sesión satisfactorio",
                Data = new LoginResponseDto
                {
                    FullName = $"{userEntity.FirstName} {userEntity.LastName}",
                    Email = userEntity.Email,       //o dto.Email
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    TokenExpiration = jwtToken.ValidTo
                }
            };

        }

        return new ResponseDto<LoginResponseDto>
        {
            Status = false,
            StatusCode = 401,
            Message = "Fallo el inicio de sesión"
        };
    }

    public async Task<ResponseDto<LoginResponseDto>> RegisterAsync (RegisterDto dto)
    {
        var user = new UserEntity
        {
            //se puede iniciar sesion con username e email
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.Email,
            Email = dto.Email,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (result.Succeeded)
        {
            var userEntity = await _userManager.FindByEmailAsync(dto.Email);
            //todos los usuarios que se registren por defecto van a tener el rol de user
            await _userManager.AddToRoleAsync(userEntity, RolesConstant.USER);

            //despues de que se registró se devuelve el token para que pueda acceder a las funcionalidad de rol

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userEntity.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", userEntity.Id),     //cada uno de estos campos se refiere 
                //new Claim("Nombre", userEntity.FullName),     //ejemplo referencial de que se pueden mandar varios campos
                new Claim(ClaimTypes.Role, RolesConstant.USER)
            };

            //metodo que creamos
            var jwtToken = GetToken(authClaims);

            return new ResponseDto<LoginResponseDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Registro de usuario realizado satisfactoriamente",
                Data = new LoginResponseDto
                {
                    FullName = $"{userEntity.FirstName} {userEntity.LastName}",
                    Email = user.Email,
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    TokenExpiration = jwtToken.ValidTo,

                }
            };

        }

        return new ResponseDto<LoginResponseDto>
        {
            StatusCode = 400,
            Status = false,
            Message = "Error al registrar el usuario"
        };
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        //SymmetricSecurityKey convierte a bites algo
        var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_configuration["JWT:Secret"]));

        return new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(1),          //probar el login en el frontend
                //expires: DateTime.Now.AddMinutes(int.Parse(_configuration["JWT:Expires"] ?? "15")),   //prueba de renovar token          
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey,
                    SecurityAlgorithms.HmacSha256)
            );

    }

    //public string GetUserId()
    //{
    //    return "7fac7ac3-4d80-4c3a-8327-5c46213d1dd3";
    //}
}
