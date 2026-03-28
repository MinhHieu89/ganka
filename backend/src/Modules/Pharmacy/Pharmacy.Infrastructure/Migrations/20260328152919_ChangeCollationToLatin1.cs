using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCollationToLatin1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "pharmacy",
                table: "Suppliers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "SQL_Latin1_General_Cp1_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "Vietnamese_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "NameVi",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "SQL_Latin1_General_Cp1_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "Vietnamese_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "SQL_Latin1_General_Cp1_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "Vietnamese_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "GenericName",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "SQL_Latin1_General_Cp1_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "Vietnamese_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "NameVi",
                schema: "pharmacy",
                table: "ConsumableItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "SQL_Latin1_General_Cp1_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "Vietnamese_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "pharmacy",
                table: "ConsumableItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "SQL_Latin1_General_Cp1_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "Vietnamese_CI_AI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "pharmacy",
                table: "Suppliers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "Vietnamese_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "SQL_Latin1_General_Cp1_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "NameVi",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "Vietnamese_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "SQL_Latin1_General_Cp1_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "Vietnamese_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "SQL_Latin1_General_Cp1_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "GenericName",
                schema: "pharmacy",
                table: "DrugCatalogItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "Vietnamese_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "SQL_Latin1_General_Cp1_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "NameVi",
                schema: "pharmacy",
                table: "ConsumableItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "Vietnamese_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "SQL_Latin1_General_Cp1_CI_AI");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "pharmacy",
                table: "ConsumableItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "Vietnamese_CI_AI",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldCollation: "SQL_Latin1_General_Cp1_CI_AI");
        }
    }
}
