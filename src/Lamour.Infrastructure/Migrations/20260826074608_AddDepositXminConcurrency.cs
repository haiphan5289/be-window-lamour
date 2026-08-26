using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositXminConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // `xmin` là cột hệ thống có sẵn trên MỌI bảng PostgreSQL — không cần (và không thể)
            // ADD COLUMN. Migration này chỉ cập nhật EF model snapshot để khai báo `xmin` làm
            // concurrency token cho `Deposit`, không đổi schema thật.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
