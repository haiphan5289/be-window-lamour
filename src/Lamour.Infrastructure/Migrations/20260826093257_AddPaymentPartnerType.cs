using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPartnerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_suppliers_SupplierId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_SupplierId",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "payments",
                newName: "PartnerId");

            migrationBuilder.AddColumn<string>(
                name: "PartnerName",
                table: "payments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartnerType",
                table: "payments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Existing rows were all Supplier-only ("Đối tượng" — backfill PartnerType + cache PartnerName
            // from the supplier now referenced by the renamed SupplierId → PartnerId column).
            migrationBuilder.Sql(@"
                UPDATE payments p
                SET ""PartnerType"" = 'Supplier',
                    ""PartnerName"" = s.name
                FROM suppliers s
                WHERE s.id = p.""PartnerId"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_payments_PartnerType_PartnerId",
                table: "payments",
                columns: new[] { "PartnerType", "PartnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payments_PartnerType_PartnerId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PartnerName",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PartnerType",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "payments",
                newName: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_SupplierId",
                table: "payments",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_suppliers_SupplierId",
                table: "payments",
                column: "SupplierId",
                principalTable: "suppliers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
