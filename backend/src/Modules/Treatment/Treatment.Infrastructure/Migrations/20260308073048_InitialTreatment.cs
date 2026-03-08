using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treatment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTreatment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "treatment");

            migrationBuilder.CreateTable(
                name: "TreatmentPackages",
                schema: "treatment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProtocolTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TreatmentType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    PricingMode = table.Column<int>(type: "int", nullable: false),
                    PackagePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SessionPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinIntervalDays = table.Column<int>(type: "int", nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentProtocols",
                schema: "treatment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TreatmentType = table.Column<int>(type: "int", nullable: false),
                    DefaultSessionCount = table.Column<int>(type: "int", nullable: false),
                    PricingMode = table.Column<int>(type: "int", nullable: false),
                    DefaultPackagePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefaultSessionPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinIntervalDays = table.Column<int>(type: "int", nullable: false),
                    MaxIntervalDays = table.Column<int>(type: "int", nullable: false),
                    DefaultParametersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CancellationDeductionPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentProtocols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CancellationRequests",
                schema: "treatment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DeductionPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancellationRequests_TreatmentPackages_TreatmentPackageId",
                        column: x => x.TreatmentPackageId,
                        principalSchema: "treatment",
                        principalTable: "TreatmentPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolVersions",
                schema: "treatment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    PreviousJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CurrentJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ChangeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChangedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocolVersions_TreatmentPackages_TreatmentPackageId",
                        column: x => x.TreatmentPackageId,
                        principalSchema: "treatment",
                        principalTable: "TreatmentPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentSessions",
                schema: "treatment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    OsdiScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    OsdiSeverity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ClinicalNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PerformedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntervalOverrideReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentSessions_TreatmentPackages_TreatmentPackageId",
                        column: x => x.TreatmentPackageId,
                        principalSchema: "treatment",
                        principalTable: "TreatmentPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionConsumables",
                schema: "treatment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumableItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumableName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionConsumables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionConsumables_TreatmentSessions_TreatmentSessionId",
                        column: x => x.TreatmentSessionId,
                        principalSchema: "treatment",
                        principalTable: "TreatmentSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CancellationRequests_TreatmentPackageId",
                schema: "treatment",
                table: "CancellationRequests",
                column: "TreatmentPackageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolVersions_TreatmentPackageId",
                schema: "treatment",
                table: "ProtocolVersions",
                column: "TreatmentPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionConsumables_ConsumableItemId",
                schema: "treatment",
                table: "SessionConsumables",
                column: "ConsumableItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionConsumables_TreatmentSessionId",
                schema: "treatment",
                table: "SessionConsumables",
                column: "TreatmentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPackages_PatientId",
                schema: "treatment",
                table: "TreatmentPackages",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPackages_ProtocolTemplateId",
                schema: "treatment",
                table: "TreatmentPackages",
                column: "ProtocolTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPackages_Status",
                schema: "treatment",
                table: "TreatmentPackages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPackages_TreatmentType",
                schema: "treatment",
                table: "TreatmentPackages",
                column: "TreatmentType");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentProtocols_IsActive",
                schema: "treatment",
                table: "TreatmentProtocols",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentProtocols_TreatmentType",
                schema: "treatment",
                table: "TreatmentProtocols",
                column: "TreatmentType");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessions_CompletedAt",
                schema: "treatment",
                table: "TreatmentSessions",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSessions_TreatmentPackageId",
                schema: "treatment",
                table: "TreatmentSessions",
                column: "TreatmentPackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancellationRequests",
                schema: "treatment");

            migrationBuilder.DropTable(
                name: "ProtocolVersions",
                schema: "treatment");

            migrationBuilder.DropTable(
                name: "SessionConsumables",
                schema: "treatment");

            migrationBuilder.DropTable(
                name: "TreatmentProtocols",
                schema: "treatment");

            migrationBuilder.DropTable(
                name: "TreatmentSessions",
                schema: "treatment");

            migrationBuilder.DropTable(
                name: "TreatmentPackages",
                schema: "treatment");
        }
    }
}
