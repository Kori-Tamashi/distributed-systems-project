using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dataaccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    customer_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    customer_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total_price = table.Column<int>(type: "integer", nullable: false),
                    booking_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    payment_method = table.Column<int>(type: "integer", nullable: false),
                    payment_transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ticket_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    flight_id = table.Column<int>(type: "integer", nullable: false),
                    passenger_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    passenger_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    passenger_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    seat_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    @class = table.Column<int>(name: "class", type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    booking_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_booking_date",
                table: "bookings",
                column: "booking_date");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_booking_reference",
                table: "bookings",
                column: "booking_reference");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_booking_uid",
                table: "bookings",
                column: "booking_uid");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_customer_email",
                table: "bookings",
                column: "customer_email");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_status",
                table: "bookings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_booking_date",
                table: "tickets",
                column: "booking_date");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_flight_id",
                table: "tickets",
                column: "flight_id");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_passenger_email",
                table: "tickets",
                column: "passenger_email");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_status",
                table: "tickets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ticket_uid",
                table: "tickets",
                column: "ticket_uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "tickets");
        }
    }
}
