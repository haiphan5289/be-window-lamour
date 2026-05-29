using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleCareToCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sale_care",
                table: "customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sale_care",
                table: "customers");
        }
    }
}
