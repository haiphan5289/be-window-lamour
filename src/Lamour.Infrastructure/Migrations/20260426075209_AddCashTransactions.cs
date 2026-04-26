using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cash_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accounting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    document_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    receipt_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    payment_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    account = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    counter_account = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    person_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cash_transactions", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "cash_transactions",
                columns: new[] { "id", "account", "accounting_date", "counter_account", "created_at", "credit_amount", "debit_amount", "description", "document_date", "payment_number", "person_name", "receipt_number" },
                values: new object[,]
                {
                    { 1, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "131", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 520000m, "Thu tiền khách hàng", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Thanh Đức", "PT00678" },
                    { 2, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "131", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 5850000m, "Thu tiền khách hàng", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Thanh Đức", "PT00678" },
                    { 3, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "131", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 5025000m, "Thu tiền khách hàng", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Thanh Đức", "PT00678" },
                    { 4, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "131", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 5640000m, "Thu tiền khách hàng", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Thanh Đức", "PT00678" },
                    { 5, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "131", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 5200000m, "Thu tiền khách hàng", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Thanh Đức", "PT00678" },
                    { 6, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "6418", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 615000m, 0m, "Diễm", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "PC02215", "LÊ HOÀNG THANH ĐỨC", null },
                    { 7, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "6418", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1055000m, 0m, "Thảo Uyên", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "PC02215", "LÊ HOÀNG THANH ĐỨC", null },
                    { 8, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "6418", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1228000m, 0m, "Phúc Nhi", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "PC02215", "LÊ HOÀNG THANH ĐỨC", null },
                    { 9, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "6418", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 174000m, 0m, "Hân", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "PC02215", "LÊ HOÀNG THANH ĐỨC", null },
                    { 10, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "6418", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 105000m, 0m, "Hương Ly", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "PC02215", "LÊ HOÀNG THANH ĐỨC", null },
                    { 11, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "6418", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 450000m, 0m, "Mua like fanpage tháng 10/2023", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "PC02216", "NGUYỄN HÀ THANH HÀ", null },
                    { 12, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "6418", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1715000m, 0m, "Phí lưu kho t10/2023", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "PC02217", "LÊ HOÀNG THANH ĐỨC", null },
                    { 13, "111", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "6418", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 40000000m, 0m, "Thuê VP t11/2023", new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), "PC02218", "LÊ HOÀNG THANH ĐỨC", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_cash_transactions_accounting_date",
                table: "cash_transactions",
                column: "accounting_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cash_transactions");
        }
    }
}
