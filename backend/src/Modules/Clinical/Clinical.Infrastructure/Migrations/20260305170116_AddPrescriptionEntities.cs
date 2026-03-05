using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrugPrescriptions",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrescriptionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PrescribedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugPrescriptions_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpticalPrescriptions",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OdSph = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OdCyl = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OdAxis = table.Column<int>(type: "int", nullable: true),
                    OdAdd = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OsSph = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OsCyl = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OsAxis = table.Column<int>(type: "int", nullable: true),
                    OsAdd = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FarPd = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NearPd = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NearOdSph = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NearOdCyl = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NearOdAxis = table.Column<int>(type: "int", nullable: true),
                    NearOsSph = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NearOsCyl = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NearOsAxis = table.Column<int>(type: "int", nullable: true),
                    LensType = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrescribedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpticalPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpticalPrescriptions_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DrugName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GenericName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Strength = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Form = table.Column<int>(type: "int", nullable: false),
                    Route = table.Column<int>(type: "int", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DosageOverride = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: true),
                    IsOffCatalog = table.Column<bool>(type: "bit", nullable: false),
                    HasAllergyWarning = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_DrugPrescriptions_DrugPrescriptionId",
                        column: x => x.DrugPrescriptionId,
                        principalSchema: "clinical",
                        principalTable: "DrugPrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugPrescriptions_VisitId",
                schema: "clinical",
                table: "DrugPrescriptions",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_OpticalPrescriptions_VisitId",
                schema: "clinical",
                table: "OpticalPrescriptions",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_DrugPrescriptionId",
                schema: "clinical",
                table: "PrescriptionItems",
                column: "DrugPrescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpticalPrescriptions",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "PrescriptionItems",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "DrugPrescriptions",
                schema: "clinical");
        }
    }
}
