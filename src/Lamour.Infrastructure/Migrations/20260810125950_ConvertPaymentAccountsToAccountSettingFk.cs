using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPaymentAccountsToAccountSettingFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditAccount",
                table: "payment_entries");

            migrationBuilder.DropColumn(
                name: "DebitAccount",
                table: "payment_entries");

            migrationBuilder.AddColumn<int>(
                name: "CreditAccountSettingId",
                table: "payment_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DebitAccountSettingId",
                table: "payment_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "account_settings",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { 40, "111", "Tiền mặt" },
                    { 41, "112", "Tiền gửi ngân hàng" },
                    { 42, "131", "Phải thu của khách hàng" },
                    { 43, "334", "Phải trả người lao động" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_entries_CreditAccountSettingId",
                table: "payment_entries",
                column: "CreditAccountSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_entries_DebitAccountSettingId",
                table: "payment_entries",
                column: "DebitAccountSettingId");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_entries_account_settings_CreditAccountSettingId",
                table: "payment_entries",
                column: "CreditAccountSettingId",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_entries_account_settings_DebitAccountSettingId",
                table: "payment_entries",
                column: "DebitAccountSettingId",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_entries_account_settings_CreditAccountSettingId",
                table: "payment_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_entries_account_settings_DebitAccountSettingId",
                table: "payment_entries");

            migrationBuilder.DropIndex(
                name: "IX_payment_entries_CreditAccountSettingId",
                table: "payment_entries");

            migrationBuilder.DropIndex(
                name: "IX_payment_entries_DebitAccountSettingId",
                table: "payment_entries");

            migrationBuilder.DeleteData(
                table: "account_settings",
                keyColumn: "id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "account_settings",
                keyColumn: "id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "account_settings",
                keyColumn: "id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "account_settings",
                keyColumn: "id",
                keyValue: 43);

            migrationBuilder.DropColumn(
                name: "CreditAccountSettingId",
                table: "payment_entries");

            migrationBuilder.DropColumn(
                name: "DebitAccountSettingId",
                table: "payment_entries");

            migrationBuilder.AddColumn<string>(
                name: "CreditAccount",
                table: "payment_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DebitAccount",
                table: "payment_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
