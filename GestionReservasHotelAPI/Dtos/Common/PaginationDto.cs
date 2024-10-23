namespace GestionReservasHotelAPI.Dtos.Common;

public class PaginationDto<T>
{
    public bool HasNextPage { get; set; }   //la pagina donde estamos tiene mas paginas delante
    public bool HasPreviousPage { get; set; } //la pagina donde estamos tiene mas paginas detras
    public int CurrentPage { get; set; }       //almacenar la pagina donde esta el cliente
    public int PageSize { get; set; }           //es un dato de configuracion, se hace desde el appsettings.Development
    public int TotalItems { get; set; }     //rows que hay en la fila de datos
    public int TotalPages { get; set; }

    //Fila de datos que vamos a devolver
    //el tipo de dato T puede ser un hotel, habitacion...
    public T Items { get; set; }
}
