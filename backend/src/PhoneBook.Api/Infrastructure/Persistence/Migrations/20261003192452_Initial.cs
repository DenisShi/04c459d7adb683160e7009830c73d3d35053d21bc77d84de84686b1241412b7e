using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhoneBook.Api.Infrastructure.Persistence.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "phone_numbers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    visibility = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    owner_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    owner_username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_phone_numbers", x => x.id);
                    table.CheckConstraint("ck_phone_numbers_number_format", "number ~ '^\\+?[0-9]{3,15}$'");
                    table.CheckConstraint("ck_phone_numbers_visibility", "visibility IN ('PERSONAL', 'SHARED')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_phone_numbers_owner_id_created_at",
                table: "phone_numbers",
                columns: new[] { "owner_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_phone_numbers_shared_created_at",
                table: "phone_numbers",
                column: "created_at",
                descending: new bool[0],
                filter: "visibility = 'SHARED'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "phone_numbers");
        }
    }
}
