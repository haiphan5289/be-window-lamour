using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesReturnTaxCostAndDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cogs_account",
                table: "sales_return_lines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "632");

            migrationBuilder.AddColumn<string>(
                name: "cost_account",
                table: "sales_return_lines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "1561");

            migrationBuilder.AddColumn<decimal>(
                name: "cost_amount",
                table: "sales_return_lines",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cost_price",
                table: "sales_return_lines",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "department_id",
                table: "sales_return_lines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_account",
                table: "sales_return_lines",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "33311");

            migrationBuilder.AddColumn<decimal>(
                name: "tax_amount",
                table: "sales_return_lines",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_rate",
                table: "sales_return_lines",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_sales_return_lines_department_id",
                table: "sales_return_lines",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_sales_return_lines_departments_department_id",
                table: "sales_return_lines",
                column: "department_id",
                principalTable: "departments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sales_return_lines_departments_department_id",
                table: "sales_return_lines");

            migrationBuilder.DropIndex(
                name: "IX_sales_return_lines_department_id",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "cogs_account",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "cost_account",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "cost_amount",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "cost_price",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "tax_account",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "tax_amount",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "tax_rate",
                table: "sales_return_lines");
        }
    }
}
