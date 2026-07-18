using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SaleCareEmployeeIdAndSalesOrderTax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "grand_total",
                table: "sales_orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_tax_amount",
                table: "sales_orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_amount",
                table: "sales_order_lines",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_rate",
                table: "sales_order_lines",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "sale_care_employee_id",
                table: "customers",
                type: "integer",
                nullable: true);

            // Backfill: preserve existing free-text sale_care by matching it to an employee name
            // before the old column is dropped below.
            migrationBuilder.Sql(@"
                UPDATE customers c
                SET sale_care_employee_id = e.id
                FROM employees e
                WHERE lower(trim(c.sale_care)) = lower(trim(e.name))
                  AND c.sale_care IS NOT NULL AND c.sale_care <> '';
            ");

            migrationBuilder.DropColumn(
                name: "sale_care",
                table: "customers");

            migrationBuilder.CreateIndex(
                name: "IX_customers_sale_care_employee_id",
                table: "customers",
                column: "sale_care_employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_customers_employees_sale_care_employee_id",
                table: "customers",
                column: "sale_care_employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customers_employees_sale_care_employee_id",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "IX_customers_sale_care_employee_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "grand_total",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "total_tax_amount",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "tax_amount",
                table: "sales_order_lines");

            migrationBuilder.DropColumn(
                name: "tax_rate",
                table: "sales_order_lines");

            migrationBuilder.DropColumn(
                name: "sale_care_employee_id",
                table: "customers");

            migrationBuilder.AddColumn<string>(
                name: "sale_care",
                table: "customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
