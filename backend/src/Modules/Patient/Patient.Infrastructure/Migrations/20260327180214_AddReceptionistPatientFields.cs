using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patient.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionistPatientFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactLensUsage",
                schema: "patient",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentMedications",
                schema: "patient",
                table: "Patients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "patient",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifestyleNotes",
                schema: "patient",
                table: "Patients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                schema: "patient",
                table: "Patients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OcularHistory",
                schema: "patient",
                table: "Patients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScreenTimeHours",
                schema: "patient",
                table: "Patients",
                type: "decimal(4,1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemicHistory",
                schema: "patient",
                table: "Patients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkEnvironment",
                schema: "patient",
                table: "Patients",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactLensUsage",
                schema: "patient",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CurrentMedications",
                schema: "patient",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "patient",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LifestyleNotes",
                schema: "patient",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Occupation",
                schema: "patient",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "OcularHistory",
                schema: "patient",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ScreenTimeHours",
                schema: "patient",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "SystemicHistory",
                schema: "patient",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "WorkEnvironment",
                schema: "patient",
                table: "Patients");
        }
    }
}
