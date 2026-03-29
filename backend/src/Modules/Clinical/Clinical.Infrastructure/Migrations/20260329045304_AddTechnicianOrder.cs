using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Source",
                schema: "clinical",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "TechnicianOrders",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TechnicianName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRedFlag = table.Column<bool>(type: "bit", nullable: false),
                    RedFlagReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RedFlaggedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderedByDoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicianOrders_Visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianOrders_TechnicianId",
                schema: "clinical",
                table: "TechnicianOrders",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianOrders_VisitId_OrderType",
                schema: "clinical",
                table: "TechnicianOrders",
                columns: new[] { "VisitId", "OrderType" },
                unique: true,
                filter: "[OrderType] = 'PreExam'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechnicianOrders",
                schema: "clinical");

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                schema: "clinical",
                table: "Visits",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);
        }
    }
}
