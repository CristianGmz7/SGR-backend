using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionReservasHotelAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsOrderIdAndCaptureIdReservationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "capture_id",
                schema: "dbo",
                table: "reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "order_id",
                schema: "dbo",
                table: "reservations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "capture_id",
                schema: "dbo",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "order_id",
                schema: "dbo",
                table: "reservations");
        }
    }
}
