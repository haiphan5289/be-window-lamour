using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameSalesOrderColumnsToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sales_order_lines_products_ProductId",
                table: "sales_order_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_order_lines_sales_orders_SalesOrderId",
                table: "sales_order_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_orders_customers_CustomerId",
                table: "sales_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_orders_employees_EmployeeId",
                table: "sales_orders");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "sales_orders",
                newName: "reference");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "sales_orders",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "sales_orders",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sales_orders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "sales_orders",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "PaymentTerms",
                table: "sales_orders",
                newName: "payment_terms");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "sales_orders",
                newName: "payment_method");

            migrationBuilder.RenameColumn(
                name: "PaymentDueDays",
                table: "sales_orders",
                newName: "payment_due_days");

            migrationBuilder.RenameColumn(
                name: "PaymentDueDate",
                table: "sales_orders",
                newName: "payment_due_date");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "sales_orders",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "DocumentNumber",
                table: "sales_orders",
                newName: "document_number");

            migrationBuilder.RenameColumn(
                name: "DocumentDate",
                table: "sales_orders",
                newName: "document_date");

            migrationBuilder.RenameColumn(
                name: "DeliveryMethod",
                table: "sales_orders",
                newName: "delivery_method");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "sales_orders",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "sales_orders",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AccountingDate",
                table: "sales_orders",
                newName: "accounting_date");

            migrationBuilder.RenameIndex(
                name: "IX_sales_orders_EmployeeId",
                table: "sales_orders",
                newName: "IX_sales_orders_employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_sales_orders_DocumentNumber",
                table: "sales_orders",
                newName: "IX_sales_orders_document_number");

            migrationBuilder.RenameIndex(
                name: "IX_sales_orders_CustomerId",
                table: "sales_orders",
                newName: "IX_sales_orders_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_sales_orders_AccountingDate",
                table: "sales_orders",
                newName: "IX_sales_orders_accounting_date");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "sales_order_lines",
                newName: "unit");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "sales_order_lines",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "sales_order_lines",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sales_order_lines",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "sales_order_lines",
                newName: "unit_price");

            migrationBuilder.RenameColumn(
                name: "SalesOrderId",
                table: "sales_order_lines",
                newName: "sales_order_id");

            migrationBuilder.RenameColumn(
                name: "RevenueAccount",
                table: "sales_order_lines",
                newName: "revenue_account");

            migrationBuilder.RenameColumn(
                name: "ReceivableAccount",
                table: "sales_order_lines",
                newName: "receivable_account");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "sales_order_lines",
                newName: "product_name");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "sales_order_lines",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "ProductCode",
                table: "sales_order_lines",
                newName: "product_code");

            migrationBuilder.RenameColumn(
                name: "IsPromotion",
                table: "sales_order_lines",
                newName: "is_promotion");

            migrationBuilder.RenameColumn(
                name: "DiscountRate",
                table: "sales_order_lines",
                newName: "discount_rate");

            migrationBuilder.RenameIndex(
                name: "IX_sales_order_lines_SalesOrderId",
                table: "sales_order_lines",
                newName: "IX_sales_order_lines_sales_order_id");

            migrationBuilder.RenameIndex(
                name: "IX_sales_order_lines_ProductId",
                table: "sales_order_lines",
                newName: "IX_sales_order_lines_product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_sales_order_lines_products_product_id",
                table: "sales_order_lines",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_order_lines_sales_orders_sales_order_id",
                table: "sales_order_lines",
                column: "sales_order_id",
                principalTable: "sales_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_orders_customers_customer_id",
                table: "sales_orders",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_orders_employees_employee_id",
                table: "sales_orders",
                column: "employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sales_order_lines_products_product_id",
                table: "sales_order_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_order_lines_sales_orders_sales_order_id",
                table: "sales_order_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_orders_customers_customer_id",
                table: "sales_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_orders_employees_employee_id",
                table: "sales_orders");

            migrationBuilder.RenameColumn(
                name: "reference",
                table: "sales_orders",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "sales_orders",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "sales_orders",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "sales_orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "sales_orders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "payment_terms",
                table: "sales_orders",
                newName: "PaymentTerms");

            migrationBuilder.RenameColumn(
                name: "payment_method",
                table: "sales_orders",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "payment_due_days",
                table: "sales_orders",
                newName: "PaymentDueDays");

            migrationBuilder.RenameColumn(
                name: "payment_due_date",
                table: "sales_orders",
                newName: "PaymentDueDate");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "sales_orders",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "document_number",
                table: "sales_orders",
                newName: "DocumentNumber");

            migrationBuilder.RenameColumn(
                name: "document_date",
                table: "sales_orders",
                newName: "DocumentDate");

            migrationBuilder.RenameColumn(
                name: "delivery_method",
                table: "sales_orders",
                newName: "DeliveryMethod");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "sales_orders",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "sales_orders",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "accounting_date",
                table: "sales_orders",
                newName: "AccountingDate");

            migrationBuilder.RenameIndex(
                name: "IX_sales_orders_employee_id",
                table: "sales_orders",
                newName: "IX_sales_orders_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_sales_orders_document_number",
                table: "sales_orders",
                newName: "IX_sales_orders_DocumentNumber");

            migrationBuilder.RenameIndex(
                name: "IX_sales_orders_customer_id",
                table: "sales_orders",
                newName: "IX_sales_orders_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_sales_orders_accounting_date",
                table: "sales_orders",
                newName: "IX_sales_orders_AccountingDate");

            migrationBuilder.RenameColumn(
                name: "unit",
                table: "sales_order_lines",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "sales_order_lines",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "sales_order_lines",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "sales_order_lines",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                table: "sales_order_lines",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "sales_order_id",
                table: "sales_order_lines",
                newName: "SalesOrderId");

            migrationBuilder.RenameColumn(
                name: "revenue_account",
                table: "sales_order_lines",
                newName: "RevenueAccount");

            migrationBuilder.RenameColumn(
                name: "receivable_account",
                table: "sales_order_lines",
                newName: "ReceivableAccount");

            migrationBuilder.RenameColumn(
                name: "product_name",
                table: "sales_order_lines",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "sales_order_lines",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "product_code",
                table: "sales_order_lines",
                newName: "ProductCode");

            migrationBuilder.RenameColumn(
                name: "is_promotion",
                table: "sales_order_lines",
                newName: "IsPromotion");

            migrationBuilder.RenameColumn(
                name: "discount_rate",
                table: "sales_order_lines",
                newName: "DiscountRate");

            migrationBuilder.RenameIndex(
                name: "IX_sales_order_lines_sales_order_id",
                table: "sales_order_lines",
                newName: "IX_sales_order_lines_SalesOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_sales_order_lines_product_id",
                table: "sales_order_lines",
                newName: "IX_sales_order_lines_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_sales_order_lines_products_ProductId",
                table: "sales_order_lines",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_order_lines_sales_orders_SalesOrderId",
                table: "sales_order_lines",
                column: "SalesOrderId",
                principalTable: "sales_orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_orders_customers_CustomerId",
                table: "sales_orders",
                column: "CustomerId",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_orders_employees_EmployeeId",
                table: "sales_orders",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
