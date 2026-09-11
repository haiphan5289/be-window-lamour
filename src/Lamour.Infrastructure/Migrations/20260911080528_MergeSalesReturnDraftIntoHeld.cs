using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergeSalesReturnDraftIntoHeld : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Theo yêu cầu: gộp "Nháp" (Draft=0) và "Treo" (Held=2) thành 1 trạng thái duy nhất
            // "Treo" — chuyển toàn bộ chứng từ đang Draft sang Held. Xác nhận qua psql trước khi
            // chạy: chỉ 3/30 chứng từ đang ở Draft (id thật, không phải seed/test cố định).
            migrationBuilder.Sql("UPDATE sales_returns SET status = 2 WHERE status = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không thể khôi phục chính xác dòng nào TRƯỚC ĐÓ là Draft thật (đã trộn lẫn với các
            // dòng vốn dĩ đã là Held từ trước migration này) — Down() không revert dữ liệu, chỉ để
            // migration có thể remove khỏi lịch sử nếu cần. Không no-op nguy hiểm vì Up() không thể
            // đảo ngược an toàn (mất thông tin), khác các migration status khác trong project vẫn
            // còn revert được nhờ biết chính xác tập id đã đổi.
        }
    }
}
