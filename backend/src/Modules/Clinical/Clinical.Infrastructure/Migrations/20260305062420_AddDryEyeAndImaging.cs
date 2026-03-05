using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDryEyeAndImaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DryEyeAssessments",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OdTbut = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OsTbut = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OdSchirmer = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OsSchirmer = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OdMeibomianGrading = table.Column<int>(type: "int", nullable: true),
                    OsMeibomianGrading = table.Column<int>(type: "int", nullable: true),
                    OdTearMeniscus = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OsTearMeniscus = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OdStaining = table.Column<int>(type: "int", nullable: true),
                    OsStaining = table.Column<int>(type: "int", nullable: true),
                    OsdiScore = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: true),
                    OsdiSeverity = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DryEyeAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DryEyeAssessments_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalImages",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EyeTag = table.Column<int>(type: "int", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OsdiSubmissions",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AnswersJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QuestionsAnswered = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    PublicToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OsdiSubmissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DryEyeAssessments_VisitId",
                schema: "clinical",
                table: "DryEyeAssessments",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalImages_VisitId",
                schema: "clinical",
                table: "MedicalImages",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalImages_VisitId_Type",
                schema: "clinical",
                table: "MedicalImages",
                columns: new[] { "VisitId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OsdiSubmissions_PublicToken",
                schema: "clinical",
                table: "OsdiSubmissions",
                column: "PublicToken",
                unique: true,
                filter: "[PublicToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OsdiSubmissions_VisitId",
                schema: "clinical",
                table: "OsdiSubmissions",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DryEyeAssessments",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "MedicalImages",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "OsdiSubmissions",
                schema: "clinical");
        }
    }
}
