using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionReservasHotelAPI.Migrations
{
    /// <inheritdoc />
    public partial class HotelReactTableAndAddFieldsCityDepartmentOnHotelTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city",
                schema: "dbo",
                table: "hotels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "department",
                schema: "dbo",
                table: "hotels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hotels_reacts",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hotel_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    reaction = table.Column<bool>(type: "bit", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    updated_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotels_reacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_hotels_reacts_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalSchema: "dbo",
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hotels_reacts_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hotels_reacts_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hotels_reacts_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hotels_reacts_created_by",
                schema: "dbo",
                table: "hotels_reacts",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_hotels_reacts_hotel_id",
                schema: "dbo",
                table: "hotels_reacts",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_hotels_reacts_updated_by",
                schema: "dbo",
                table: "hotels_reacts",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_hotels_reacts_user_id",
                schema: "dbo",
                table: "hotels_reacts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hotels_reacts",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "city",
                schema: "dbo",
                table: "hotels");

            migrationBuilder.DropColumn(
                name: "department",
                schema: "dbo",
                table: "hotels");
        }
    }
}
