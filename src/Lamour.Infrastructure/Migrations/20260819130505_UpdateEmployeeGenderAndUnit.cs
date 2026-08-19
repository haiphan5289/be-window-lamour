using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeGenderAndUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "unit",
                table: "employees",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "gender",
                table: "employees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Nam");

            // Data migration: remap giá trị Unit cũ (enum member name: PGD/PKD/Spa/GD/Kho) sang
            // 7 giá trị Vietnamese text mới — enum member không chứa được dấu/khoảng trắng nên
            // Unit đổi từ enum sang string tự do (xem Employee.cs, EmployeeUnits.AllowedValues).
            migrationBuilder.Sql(@"
                UPDATE employees SET unit = 'Phòng Giám Đốc'  WHERE unit = 'PGD';
                UPDATE employees SET unit = 'Phòng Kinh Doanh' WHERE unit = 'PKD';
                UPDATE employees SET unit = 'Tiệm spa'         WHERE unit = 'Spa';
                UPDATE employees SET unit = 'Phòng Giám Đốc'  WHERE unit = 'GD';
                UPDATE employees SET unit = 'Kho và Quỹ'      WHERE unit = 'Kho';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gender",
                table: "employees");

            migrationBuilder.AlterColumn<string>(
                name: "unit",
                table: "employees",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "phone",
                table: "employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "");
        }
    }
}
