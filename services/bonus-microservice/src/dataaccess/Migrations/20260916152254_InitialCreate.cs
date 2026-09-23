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
                name: "privilege",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    balance = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privilege", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "privilege_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    privilege_id = table.Column<int>(type: "integer", nullable: false),
                    ticket_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    balance_diff = table.Column<int>(type: "integer", nullable: false),
                    operation_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_privilege_history", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_privilege_status",
                table: "privilege",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_privilege_username",
                table: "privilege",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_privilege_history_datetime",
                table: "privilege_history",
                column: "datetime");

            migrationBuilder.CreateIndex(
                name: "IX_privilege_history_operation_type",
                table: "privilege_history",
                column: "operation_type");

            migrationBuilder.CreateIndex(
                name: "IX_privilege_history_privilege_id",
                table: "privilege_history",
                column: "privilege_id");

            migrationBuilder.CreateIndex(
                name: "IX_privilege_history_ticket_uid",
                table: "privilege_history",
                column: "ticket_uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "privilege");

            migrationBuilder.DropTable(
                name: "privilege_history");
        }
    }
}
