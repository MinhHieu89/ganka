using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionistAppointmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                schema: "scheduling",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledBy",
                schema: "scheduling",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                schema: "scheduling",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                schema: "scheduling",
                table: "Appointments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestPhone",
                schema: "scheduling",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestReason",
                schema: "scheduling",
                table: "Appointments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NoShowAt",
                schema: "scheduling",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NoShowBy",
                schema: "scheduling",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoShowNotes",
                schema: "scheduling",
                table: "Appointments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                schema: "scheduling",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledBy",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "GuestName",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "GuestPhone",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "GuestReason",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "NoShowAt",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "NoShowBy",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "NoShowNotes",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "scheduling",
                table: "Appointments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                schema: "scheduling",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
