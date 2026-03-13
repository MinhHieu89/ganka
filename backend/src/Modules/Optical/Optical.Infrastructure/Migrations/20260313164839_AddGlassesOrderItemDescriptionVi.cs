using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Optical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGlassesOrderItemDescriptionVi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemDescriptionVi",
                schema: "optical",
                table: "GlassesOrderItems",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemDescriptionVi",
                schema: "optical",
                table: "GlassesOrderItems");
        }
    }
}
