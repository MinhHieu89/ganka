using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.Reference
{
    /// <inheritdoc />
    public partial class SetLatin1CollationForVietnameseSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DescriptionVi",
                schema: "reference",
                table: "Icd10Codes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                collation: "Latin1_General_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldCollation: "Vietnamese_CI_AI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DescriptionVi",
                schema: "reference",
                table: "Icd10Codes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                collation: "Vietnamese_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldCollation: "Latin1_General_CI_AI");
        }
    }
}
