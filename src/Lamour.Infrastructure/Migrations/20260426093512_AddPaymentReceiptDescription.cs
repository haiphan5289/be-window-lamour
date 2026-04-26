using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReceiptDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "payment_receipts",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "unit",
                table: "employees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "'Spa'");

            migrationBuilder.AlterColumn<string>(
                name: "job_title",
                table: "employees",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldDefaultValueSql: "'Khac'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "payment_receipts");

            migrationBuilder.AlterColumn<string>(
                name: "unit",
                table: "employees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "'Spa'",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "job_title",
                table: "employees",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValueSql: "'Khac'",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
