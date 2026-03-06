using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PharmacyInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumableItems",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Vietnamese_CI_AI"),
                    NameVi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Vietnamese_CI_AI"),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrackingMode = table.Column<int>(type: "int", nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DispensingRecords",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DispensedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OverrideReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispensingRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtcSales",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoldById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoldAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtcSales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustments",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConsumableBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuantityChange = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdjustedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdjustedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsumableBatches",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumableItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InitialQuantity = table.Column<int>(type: "int", nullable: false),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumableBatches_ConsumableItems_ConsumableItemId",
                        column: x => x.ConsumableItemId,
                        principalSchema: "pharmacy",
                        principalTable: "ConsumableItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DispensingLines",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispensingRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispensingLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispensingLines_DispensingRecords_DispensingRecordId",
                        column: x => x.DispensingRecordId,
                        principalSchema: "pharmacy",
                        principalTable: "DispensingRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtcSaleLines",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OtcSaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtcSaleLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtcSaleLines_OtcSales_OtcSaleId",
                        column: x => x.OtcSaleId,
                        principalSchema: "pharmacy",
                        principalTable: "OtcSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BatchDeductions",
                schema: "pharmacy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispensingLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OtcSaleLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DrugBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchDeductions_DispensingLines_DispensingLineId",
                        column: x => x.DispensingLineId,
                        principalSchema: "pharmacy",
                        principalTable: "DispensingLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchDeductions_OtcSaleLines_OtcSaleLineId",
                        column: x => x.OtcSaleLineId,
                        principalSchema: "pharmacy",
                        principalTable: "OtcSaleLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchDeductions_DispensingLineId",
                schema: "pharmacy",
                table: "BatchDeductions",
                column: "DispensingLineId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchDeductions_OtcSaleLineId",
                schema: "pharmacy",
                table: "BatchDeductions",
                column: "OtcSaleLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableBatches_ConsumableItemId_ExpiryDate",
                schema: "pharmacy",
                table: "ConsumableBatches",
                columns: new[] { "ConsumableItemId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableItems_Name",
                schema: "pharmacy",
                table: "ConsumableItems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DispensingLines_DispensingRecordId",
                schema: "pharmacy",
                table: "DispensingLines",
                column: "DispensingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_OtcSaleLines_OtcSaleId",
                schema: "pharmacy",
                table: "OtcSaleLines",
                column: "OtcSaleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchDeductions",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "ConsumableBatches",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "StockAdjustments",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "DispensingLines",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "OtcSaleLines",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "ConsumableItems",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "DispensingRecords",
                schema: "pharmacy");

            migrationBuilder.DropTable(
                name: "OtcSales",
                schema: "pharmacy");
        }
    }
}
