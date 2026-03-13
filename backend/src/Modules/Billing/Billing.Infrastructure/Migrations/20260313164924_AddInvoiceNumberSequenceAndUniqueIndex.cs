using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceNumberSequenceAndUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create SQL SEQUENCE for atomic invoice number generation (replaces MAX+1 pattern)
            migrationBuilder.Sql(
                "CREATE SEQUENCE billing.InvoiceNumberSeq START WITH 1 INCREMENT BY 1;");

            // If existing invoices exist, advance the sequence past the highest number
            migrationBuilder.Sql(@"
                DECLARE @max INT = (
                    SELECT ISNULL(MAX(CAST(RIGHT(InvoiceNumber, 5) AS INT)), 0)
                    FROM billing.Invoices
                );
                IF @max > 0
                BEGIN
                    DECLARE @next INT = @max + 1;
                    DECLARE @sql NVARCHAR(200) = N'ALTER SEQUENCE billing.InvoiceNumberSeq RESTART WITH ' + CAST(@next AS NVARCHAR(10));
                    EXEC sp_executesql @sql;
                END
            ");

            // Note: Unique index IX_Invoices_InvoiceNumber already exists from InitialBilling migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS billing.InvoiceNumberSeq;");
        }
    }
}
