using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentStatusAndExpenseCategoryLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonDetail",
                table: "payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "payments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExpenseCategoryId",
                table: "payment_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_entries_ExpenseCategoryId",
                table: "payment_entries",
                column: "ExpenseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_entries_expense_categories_ExpenseCategoryId",
                table: "payment_entries",
                column: "ExpenseCategoryId",
                principalTable: "expense_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_entries_expense_categories_ExpenseCategoryId",
                table: "payment_entries");

            migrationBuilder.DropIndex(
                name: "IX_payment_entries_ExpenseCategoryId",
                table: "payment_entries");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ReasonDetail",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ExpenseCategoryId",
                table: "payment_entries");
        }
    }
}
