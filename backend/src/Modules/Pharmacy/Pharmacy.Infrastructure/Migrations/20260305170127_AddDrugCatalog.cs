using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDrugCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pharmacy");

            migrationBuilder.CreateTable(
                name: "DrugCatalogItems",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Vietnamese_CI_AI"),
                    NameVi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Vietnamese_CI_AI"),
                    GenericName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Vietnamese_CI_AI"),
                    Form = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Route = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefaultDosageTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugCatalogItems_GenericName",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                column: "GenericName");

            migrationBuilder.CreateIndex(
                name: "IX_DrugCatalogItems_Name",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugCatalogItems",
                schema: "pharmacy");
        }
    }
}
