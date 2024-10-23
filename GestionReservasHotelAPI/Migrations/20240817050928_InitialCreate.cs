using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionReservasHotelAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "condition",
                schema: "dbo",
                table: "reservations");

            migrationBuilder.RenameColumn(
                name: "condition",
                schema: "dbo",
                table: "rooms",
                newName: "image_url");

            migrationBuilder.AlterColumn<string>(
                name: "address",
                schema: "dbo",
                table: "hotels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "dbo",
                table: "hotels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                schema: "dbo",
                table: "hotels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "overview",
                schema: "dbo",
                table: "hotels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                schema: "dbo",
                table: "hotels");

            migrationBuilder.DropColumn(
                name: "image_url",
                schema: "dbo",
                table: "hotels");

            migrationBuilder.DropColumn(
                name: "overview",
                schema: "dbo",
                table: "hotels");

            migrationBuilder.RenameColumn(
                name: "image_url",
                schema: "dbo",
                table: "rooms",
                newName: "condition");

            migrationBuilder.AddColumn<string>(
                name: "condition",
                schema: "dbo",
                table: "reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "address",
                schema: "dbo",
                table: "hotels",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
