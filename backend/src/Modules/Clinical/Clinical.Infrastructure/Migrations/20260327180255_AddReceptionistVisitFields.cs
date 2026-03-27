using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionistVisitFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CancelledBy",
                schema: "clinical",
                table: "Visits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledReason",
                schema: "clinical",
                table: "Visits",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "clinical",
                table: "Visits",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                schema: "clinical",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledBy",
                schema: "clinical",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CancelledReason",
                schema: "clinical",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "clinical",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "clinical",
                table: "Visits");
        }
    }
}
