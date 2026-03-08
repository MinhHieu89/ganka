using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Optical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOpticalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "optical");

            migrationBuilder.CreateTable(
                name: "ComboPackages",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FrameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LensCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ComboPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OriginalTotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Frames",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LensWidth = table.Column<int>(type: "int", nullable: false),
                    BridgeWidth = table.Column<int>(type: "int", nullable: false),
                    TempleLength = table.Column<int>(type: "int", nullable: false),
                    Material = table.Column<int>(type: "int", nullable: false),
                    FrameType = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlassesOrders",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpticalPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProcessingType = table.Column<int>(type: "int", nullable: false),
                    IsPaymentConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComboPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlassesOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LensCatalogItems",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LensType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Material = table.Column<int>(type: "int", nullable: false),
                    AvailableCoatings = table.Column<int>(type: "int", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PreferredSupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LensCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LensOrders",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LensCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GlassesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sph = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Cyl = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Add = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Axis = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    RequestedCoatings = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LensOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StocktakingSessions",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StocktakingSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyClaims",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GlassesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resolution = table.Column<int>(type: "int", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    AssessmentNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentUrls = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlassesOrderItems",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GlassesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FrameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LensCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlassesOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlassesOrderItems_GlassesOrders_GlassesOrderId",
                        column: x => x.GlassesOrderId,
                        principalSchema: "optical",
                        principalTable: "GlassesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LensStockEntries",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LensCatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sph = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Cyl = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Add = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LensStockEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LensStockEntries_LensCatalogItems_LensCatalogItemId",
                        column: x => x.LensCatalogItemId,
                        principalSchema: "optical",
                        principalTable: "LensCatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StocktakingItems",
                schema: "optical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StocktakingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    FrameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FrameName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PhysicalCount = table.Column<int>(type: "int", nullable: false),
                    SystemCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StocktakingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StocktakingItems_StocktakingSessions_StocktakingSessionId",
                        column: x => x.StocktakingSessionId,
                        principalSchema: "optical",
                        principalTable: "StocktakingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComboPackages_IsActive",
                schema: "optical",
                table: "ComboPackages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ComboPackages_Name",
                schema: "optical",
                table: "ComboPackages",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Frames_Barcode",
                schema: "optical",
                table: "Frames",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Frames_Brand",
                schema: "optical",
                table: "Frames",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_Frames_IsActive",
                schema: "optical",
                table: "Frames",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GlassesOrderItems_GlassesOrderId",
                schema: "optical",
                table: "GlassesOrderItems",
                column: "GlassesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GlassesOrders_PatientId",
                schema: "optical",
                table: "GlassesOrders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_GlassesOrders_Status",
                schema: "optical",
                table: "GlassesOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GlassesOrders_VisitId",
                schema: "optical",
                table: "GlassesOrders",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_LensCatalogItems_Brand",
                schema: "optical",
                table: "LensCatalogItems",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_LensCatalogItems_IsActive",
                schema: "optical",
                table: "LensCatalogItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LensOrders_GlassesOrderId",
                schema: "optical",
                table: "LensOrders",
                column: "GlassesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LensOrders_PatientId",
                schema: "optical",
                table: "LensOrders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_LensOrders_Status",
                schema: "optical",
                table: "LensOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LensStockEntries_LensCatalogItemId_Sph_Cyl_Add",
                schema: "optical",
                table: "LensStockEntries",
                columns: new[] { "LensCatalogItemId", "Sph", "Cyl", "Add" },
                unique: true,
                filter: "[Add] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StocktakingItems_StocktakingSessionId_Barcode",
                schema: "optical",
                table: "StocktakingItems",
                columns: new[] { "StocktakingSessionId", "Barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StocktakingSessions_Status",
                schema: "optical",
                table: "StocktakingSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_ApprovalStatus",
                schema: "optical",
                table: "WarrantyClaims",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_GlassesOrderId",
                schema: "optical",
                table: "WarrantyClaims",
                column: "GlassesOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComboPackages",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "Frames",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "GlassesOrderItems",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "LensOrders",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "LensStockEntries",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "StocktakingItems",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "WarrantyClaims",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "GlassesOrders",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "LensCatalogItems",
                schema: "optical");

            migrationBuilder.DropTable(
                name: "StocktakingSessions",
                schema: "optical");
        }
    }
}
