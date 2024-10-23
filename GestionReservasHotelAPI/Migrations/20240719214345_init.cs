using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionReservasHotelAPI.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "hotels",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    stars_michelin = table.Column<int>(type: "int", nullable: false),
                    number_phone = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    finish_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    condition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<double>(type: "float", nullable: false),
                    client_id = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "additional_sevices",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    price = table.Column<double>(type: "float", nullable: false),
                    hotel_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_additional_sevices", x => x.id);
                    table.ForeignKey(
                        name: "FK_additional_sevices_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalSchema: "dbo",
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    number_room = table.Column<int>(type: "int", nullable: false),
                    type_room = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price_night = table.Column<double>(type: "float", nullable: false),
                    condition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hotel_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.id);
                    table.ForeignKey(
                        name: "FK_rooms_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalSchema: "dbo",
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "additional_services_reservations",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    additional_service_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reservation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_additional_services_reservations", x => x.id);
                    table.ForeignKey(
                        name: "FK_additional_services_reservations_additional_sevices_additional_service_id",
                        column: x => x.additional_service_id,
                        principalSchema: "dbo",
                        principalTable: "additional_sevices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_additional_services_reservations_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalSchema: "dbo",
                        principalTable: "reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms_reservations",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    room_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reservation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms_reservations", x => x.id);
                    table.ForeignKey(
                        name: "FK_rooms_reservations_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalSchema: "dbo",
                        principalTable: "reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rooms_reservations_rooms_room_id",
                        column: x => x.room_id,
                        principalSchema: "dbo",
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_additional_services_reservations_additional_service_id",
                schema: "dbo",
                table: "additional_services_reservations",
                column: "additional_service_id");

            migrationBuilder.CreateIndex(
                name: "IX_additional_services_reservations_reservation_id",
                schema: "dbo",
                table: "additional_services_reservations",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_additional_sevices_hotel_id",
                schema: "dbo",
                table: "additional_sevices",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_hotel_id",
                schema: "dbo",
                table: "rooms",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_reservations_reservation_id",
                schema: "dbo",
                table: "rooms_reservations",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_reservations_room_id",
                schema: "dbo",
                table: "rooms_reservations",
                column: "room_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "additional_services_reservations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "rooms_reservations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "additional_sevices",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "reservations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "rooms",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hotels",
                schema: "dbo");
        }
    }
}
