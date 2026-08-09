using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeposits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deposits",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    accounting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    document_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    remaining_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposits", x => x.id);
                    table.ForeignKey(
                        name: "FK_deposits_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_deposits_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "deposit_deductions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    deposit_id = table.Column<int>(type: "integer", nullable: false),
                    sales_order_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    accounting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    document_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit_deductions", x => x.id);
                    table.ForeignKey(
                        name: "FK_deposit_deductions_deposits_deposit_id",
                        column: x => x.deposit_id,
                        principalTable: "deposits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_deposit_deductions_sales_orders_sales_order_id",
                        column: x => x.sales_order_id,
                        principalTable: "sales_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deposit_deductions_accounting_date",
                table: "deposit_deductions",
                column: "accounting_date");

            migrationBuilder.CreateIndex(
                name: "IX_deposit_deductions_deposit_id",
                table: "deposit_deductions",
                column: "deposit_id");

            migrationBuilder.CreateIndex(
                name: "IX_deposit_deductions_document_number",
                table: "deposit_deductions",
                column: "document_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deposit_deductions_sales_order_id",
                table: "deposit_deductions",
                column: "sales_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_deposits_accounting_date",
                table: "deposits",
                column: "accounting_date");

            migrationBuilder.CreateIndex(
                name: "IX_deposits_customer_id",
                table: "deposits",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_deposits_document_number",
                table: "deposits",
                column: "document_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deposits_employee_id",
                table: "deposits",
                column: "employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deposit_deductions");

            migrationBuilder.DropTable(
                name: "deposits");
        }
    }
}
