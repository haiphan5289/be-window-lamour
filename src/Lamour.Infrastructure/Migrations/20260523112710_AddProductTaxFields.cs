using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTaxFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "excise_tax_group",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "export_tax_rate",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "import_tax_rate",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_reduction_type",
                table: "products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vat_rate",
                table: "products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "excise_tax_group",
                table: "products");

            migrationBuilder.DropColumn(
                name: "export_tax_rate",
                table: "products");

            migrationBuilder.DropColumn(
                name: "import_tax_rate",
                table: "products");

            migrationBuilder.DropColumn(
                name: "tax_reduction_type",
                table: "products");

            migrationBuilder.DropColumn(
                name: "vat_rate",
                table: "products");
        }
    }
}
