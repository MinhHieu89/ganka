using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyStockEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DrugBatches",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InitialQuantity = table.Column<int>(type: "int", nullable: false),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockImportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockImports",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImportSource = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImportedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDrugPrices",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDrugPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Vietnamese_CI_AI"),
                    ContactInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockImportLines",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockImportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockImportLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockImportLines_StockImports_StockImportId",
                        column: x => x.StockImportId,
                        principalSchema: "pharmacy",
                        principalTable: "StockImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugBatches_DrugCatalogItemId_ExpiryDate",
                schema: "pharmacy",
                table: "DrugBatches",
                columns: new[] { "DrugCatalogItemId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DrugBatches_ExpiryDate",
                schema: "pharmacy",
                table: "DrugBatches",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_StockImportLines_StockImportId",
                schema: "pharmacy",
                table: "StockImportLines",
                column: "StockImportId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDrugPrices_SupplierId_DrugCatalogItemId",
                schema: "pharmacy",
                table: "SupplierDrugPrices",
                columns: new[] { "SupplierId", "DrugCatalogItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                schema: "pharmacy",
                table: "Suppliers",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugBatches",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "StockImportLines",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "SupplierDrugPrices",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "StockImports",
                schema: "pharmacy");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                schema: "pharmacy",
                table: "DrugCatalogItems");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                schema: "pharmacy",
                table: "DrugCatalogItems");
        }
    }
}
