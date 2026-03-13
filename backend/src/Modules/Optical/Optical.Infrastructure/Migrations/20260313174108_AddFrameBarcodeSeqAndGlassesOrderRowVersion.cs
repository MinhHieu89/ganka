using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Optical.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFrameBarcodeSeqAndGlassesOrderRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "optical",
                table: "GlassesOrders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            // Create SQL SEQUENCE for concurrency-safe EAN-13 barcode generation (CR-02 pattern)
            migrationBuilder.Sql(
                "CREATE SEQUENCE optical.FrameBarcodeSeq AS bigint START WITH 1 INCREMENT BY 1;");

            // Seed sequence to current frame count to avoid collisions with existing barcodes
            migrationBuilder.Sql(@"
                DECLARE @currentCount bigint = (SELECT COUNT(*) FROM optical.Frames);
                IF @currentCount > 0
                    EXEC('ALTER SEQUENCE optical.FrameBarcodeSeq RESTART WITH ' + @currentCount);
            ");

            // Add unique index for single-vision lenses (Add IS NULL) to close uniqueness gap
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IX_LensStockEntries_CatalogItemId_Sph_Cyl_NullAdd
                ON optical.LensStockEntries (LensCatalogItemId, Sph, Cyl)
                WHERE [Add] IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_LensStockEntries_CatalogItemId_Sph_Cyl_NullAdd ON optical.LensStockEntries;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS optical.FrameBarcodeSeq;");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "optical",
                table: "GlassesOrders");

        }
    }
}
