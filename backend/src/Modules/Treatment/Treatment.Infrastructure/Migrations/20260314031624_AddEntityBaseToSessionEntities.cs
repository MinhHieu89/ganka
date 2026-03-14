using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treatment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityBaseToSessionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "treatment",
                table: "TreatmentSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "treatment",
                table: "TreatmentSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "treatment",
                table: "SessionConsumables",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "treatment",
                table: "SessionConsumables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "treatment",
                table: "SessionConsumables",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "treatment",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "treatment",
                table: "TreatmentSessions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "treatment",
                table: "SessionConsumables");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "treatment",
                table: "SessionConsumables");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "treatment",
                table: "SessionConsumables");
        }
    }
}
