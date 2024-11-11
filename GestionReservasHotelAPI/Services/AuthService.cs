using GestionReservasHotelAPI.Constants;
using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Dtos.Auth;
using GestionReservasHotelAPI.Dtos.Common;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GestionReservasHotelAPI.Services;

public class AuthService : IAuthService
{
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IConfiguration _configuration;
    private readonly GestionReservasHotelContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            IConfiguration configuration,
            GestionReservasHotelContext context,
            ILogger<AuthService> logger
        )
    {
        this._signInManager = signInManager;
        this._userManager = userManager;
        this._configuration = configuration;
        this._context = context;
        this._logger = logger;
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

            List<Claim> authClaims = await GetClaims(userEntity);

            var jwtToken = GetToken(authClaims);

            var refreshToken = GenerateRefreshTokenString();

            //guardar refreshtoken en la base de datos
            userEntity.RefreshToken = refreshToken;
            userEntity.RefreshTokenExpire = DateTime.Now
                .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

            _context.Entry(userEntity);
            await _context.SaveChangesAsync();

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
                    TokenExpiration = jwtToken.ValidTo,
                    RefreshToken = refreshToken
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

            var authClaims = await GetClaims(userEntity);

            //metodo que creamos
            var jwtToken = GetToken(authClaims);

            var refreshToken = GenerateRefreshTokenString();

            userEntity.RefreshToken = refreshToken;
            userEntity.RefreshTokenExpire = DateTime.Now
                .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

            _context.Entry(userEntity);
            await _context.SaveChangesAsync();

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
                    RefreshToken = refreshToken
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
                //expires: DateTime.Now.AddHours(1),          //probar el login en el frontend
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["JWT:Expires"] ?? "15")),   //prueba de renovar token          
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey,
                    SecurityAlgorithms.HmacSha256)
            );

    }

    private async Task<List<Claim>> GetClaims(UserEntity userEntity)
    {
        var authClaims = new List<Claim>
            {
                //esto no esta hasheado se manda como un base64
                new Claim(ClaimTypes.Email, userEntity.Email),  //email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  //guid unico
                new Claim("UserId", userEntity.Id)  //id 
            };

        //ver que roles tiene asignado el usuario
        var userRoles = await _userManager.GetRolesAsync(userEntity);

        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        return authClaims;
    }

    public async Task<ResponseDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
    {
        //throw new Exception("Error de prueba");
        string email = "";

        try
        {
            var principal = GetTokenPrincipal(dto.Token);

            // esto se consigue desde el jwtio 
            var emailClaim = principal.Claims.FirstOrDefault(c =>
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");        //manera de microsoft
            var userIdClaim = principal.Claims.Where(x => x.Type == "UserId").FirstOrDefault();     //manera que nosotros implementamos

            //_logger.LogInformation($"Correo del usuario es: {emailClaim.Value}");
            //_logger.LogInformation($"Id del usuario es: {userIdClaim.Value}");

            if (emailClaim is null)
            {
                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 401,
                    Status = false,
                    Message = "Acceso no autorizado. No se encontró un correo valido"
                };
            }

            email = emailClaim.Value;
            //busqueda en los usuarios al que pertenece el usuario
            var userEntity = await _userManager.FindByEmailAsync(email);

            if (userEntity is null)
            {
                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 401,
                    Status = false,
                    Message = "Acceso no autorizado. El usuario no existe"
                };
            }

            if (userEntity.RefreshToken != dto.RefreshToken)
            {
                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 401,
                    Status = false,
                    Message = "Acceso no autorizado. La sesión no es valida"
                };
            }

            if (userEntity.RefreshTokenExpire < DateTime.Now)
            {
                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 401,
                    Status = false,
                    Message = "Acceso no autorizado. La sesión ha expirado"
                };
            }

            List<Claim> authClaims = await GetClaims(userEntity);

            var jwtToken = GetToken(authClaims);

            var loginResponseDto = new LoginResponseDto
            {
                Email = email,
                FullName = $"{userEntity.FirstName} {userEntity.LastName}",
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                TokenExpiration = jwtToken.ValidTo,
                RefreshToken = GenerateRefreshTokenString()
            };

            //guardar refreshtoken en la base de datos
            userEntity.RefreshToken = loginResponseDto.RefreshToken;
            userEntity.RefreshTokenExpire = DateTime.Now
                .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

            // ¿actualizar?
            _context.Entry(userEntity);
            await _context.SaveChangesAsync();

            return new ResponseDto<LoginResponseDto>
            {
                Status = true,
                StatusCode = 200,
                Message = "Token renovado satisfactoriamente",
                Data = loginResponseDto
            };
        }
        catch (Exception e)
        {
            _logger.LogError(exception: e, message: e.Message);
            return new ResponseDto<LoginResponseDto>
            {
                StatusCode = 500,
                Status = false,
                Message = "Ocurrió un error al renovar el token"
            };
        }
    }

    public ClaimsPrincipal GetTokenPrincipal(string token)
    {
        //decodificar el token
        var securityKey = new SymmetricSecurityKey(Encoding
            .UTF8.GetBytes(_configuration.GetSection("JWT:Secret").Value));

        var validation = new TokenValidationParameters
        {
            IssuerSigningKey = securityKey,
            ValidateLifetime = false,
            ValidateActor = false,
            ValidateIssuer = false,
            ValidateAudience = false

        };

        // se crea la instancia y retorna el token
        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }

    private string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];

        using (var numberGenerator = RandomNumberGenerator.Create())
        {
            numberGenerator.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }

    //public string GetUserId()
    //{
    //    return "7fac7ac3-4d80-4c3a-8327-5c46213d1dd3";
    //}
}
