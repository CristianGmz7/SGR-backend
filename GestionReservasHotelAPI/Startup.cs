using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Database.Entities;
using GestionReservasHotelAPI.Helpers;
using GestionReservasHotelAPI.Services;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GestionReservasHotelAPI;

public class Startup
{
    private IConfiguration Configuration { get; }
    //Esta variable accede al appseseting.Development Json y se pasa en services.AddDbContext

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        //agregar context a las peticiones http
        services.AddHttpContextAccessor();

        //Add DbContext (comienza configuracion de base de datos)
        services.AddDbContext<GestionReservasHotelContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // Add custom services
        services.AddTransient<IHotelsService, HotelsService>();
        services.AddTransient<IRoomsService, RoomsService>();
        services.AddTransient<IAdditionalServicesServices, AdditionalServicesServices>();
        services.AddTransient<IReservationsService, ReservationsService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IAuditService, AuditService>();
        //falta que implementar servicio de Audit


        //AddIdentity
        services.AddIdentity<UserEntity, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        }).AddEntityFrameworkStores<GestionReservasHotelContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidAudience = Configuration["JWT:ValidAudience"],
                ValidIssuer = Configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"])),    //llave 
                ClockSkew = TimeSpan.Zero
            };
        });

        // Add AutoMapper
        services.AddAutoMapper(typeof(AutoMapperProfile));

        services.AddCors(opt =>
        {
            var allowURLS = Configuration.GetSection("AllowURLS").Get<string[]>();
            opt.AddPolicy("CorsPolicy", builder => builder
            .WithOrigins(allowURLS)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
        });

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        //aqui se agrega lo del cors
        app.UseCors("CorsPolicy");

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
