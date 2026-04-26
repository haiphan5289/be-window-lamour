using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "employees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            // Backfill existing employees with unique NV-prefixed codes using a CTE.
            migrationBuilder.Sql(@"
                WITH ranked AS (
                    SELECT id, ROW_NUMBER() OVER (ORDER BY id) AS rn
                    FROM employees
                    WHERE code = ''
                )
                UPDATE employees
                SET code = 'NV' || LPAD(CAST(ranked.rn AS TEXT), 5, '0')
                FROM ranked
                WHERE employees.id = ranked.id;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_employees_code",
                table: "employees",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_employees_code",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "code",
                table: "employees");
        }
    }
}
