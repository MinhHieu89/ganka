using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowSpecDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DrugTrackStatus",
                schema: "clinical",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GlassesTrackStatus",
                schema: "clinical",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasGlassesPrescription",
                schema: "clinical",
                table: "Visits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ImagingRequested",
                schema: "clinical",
                table: "Visits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RefractionSkipped",
                schema: "clinical",
                table: "Visits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "HandoffChecklist",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionVerified = table.Column<bool>(type: "bit", nullable: false),
                    FrameCorrect = table.Column<bool>(type: "bit", nullable: false),
                    PatientConfirmedFit = table.Column<bool>(type: "bit", nullable: false),
                    CompletedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoffChecklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HandoffChecklist_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImagingRequest",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagingRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagingRequest_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpticalOrder",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LensType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrameCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LensCostPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FrameCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsultantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsultantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpticalOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpticalOrder_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyDispensing",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacistName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispenseNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyDispensing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyDispensing_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageSkip",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    FreeTextNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SkippedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUndone = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageSkip", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageSkip_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitPayment",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentKind = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    AmountReceived = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChangeGiven = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitPayment_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImagingService",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagingRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EyeScope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    TechnicianNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagingService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagingService_ImagingRequest_ImagingRequestId",
                        column: x => x.ImagingRequestId,
                        principalSchema: "clinical",
                        principalTable: "ImagingRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DispensingLineItem",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PharmacyDispensingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Instruction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDispensed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispensingLineItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispensingLineItem_PharmacyDispensing_PharmacyDispensingId",
                        column: x => x.PharmacyDispensingId,
                        principalSchema: "clinical",
                        principalTable: "PharmacyDispensing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispensingLineItem_PharmacyDispensingId",
                schema: "clinical",
                table: "DispensingLineItem",
                column: "PharmacyDispensingId");

            migrationBuilder.CreateIndex(
                name: "IX_HandoffChecklist_VisitId",
                schema: "clinical",
                table: "HandoffChecklist",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagingRequest_VisitId",
                schema: "clinical",
                table: "ImagingRequest",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagingService_ImagingRequestId",
                schema: "clinical",
                table: "ImagingService",
                column: "ImagingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_OpticalOrder_VisitId",
                schema: "clinical",
                table: "OpticalOrder",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDispensing_VisitId",
                schema: "clinical",
                table: "PharmacyDispensing",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_StageSkip_VisitId",
                schema: "clinical",
                table: "StageSkip",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitPayment_VisitId",
                schema: "clinical",
                table: "VisitPayment",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispensingLineItem",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "HandoffChecklist",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "ImagingService",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "OpticalOrder",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "StageSkip",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "VisitPayment",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "PharmacyDispensing",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "ImagingRequest",
                schema: "clinical");

            migrationBuilder.DropColumn(
                name: "DrugTrackStatus",
                schema: "clinical",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "GlassesTrackStatus",
                schema: "clinical",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "HasGlassesPrescription",
                schema: "clinical",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ImagingRequested",
                schema: "clinical",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "RefractionSkipped",
                schema: "clinical",
                table: "Visits");
        }
    }
}
