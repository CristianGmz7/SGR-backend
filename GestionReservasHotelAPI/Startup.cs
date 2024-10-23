using GestionReservasHotelAPI.Database;
using GestionReservasHotelAPI.Helpers;
using GestionReservasHotelAPI.Services;
using GestionReservasHotelAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        //Add DbContext (comienza configuracion de base de datos)
        services.AddDbContext<GestionReservasHotelContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // Add custom services
        services.AddTransient<IHotelsService, HotelsService>();
        services.AddTransient<IRoomsService, RoomsService>();
        services.AddTransient<IAdditionalServicesServices, AdditionalServicesServices>();
        services.AddTransient<IReservationsService, ReservationsService>();
        services.AddTransient<IAuthService, AuthService>();

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

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
