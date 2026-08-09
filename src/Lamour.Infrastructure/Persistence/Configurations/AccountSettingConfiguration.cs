using Lamour.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamour.Infrastructure.Persistence.Configurations;

public class AccountSettingConfiguration : IEntityTypeConfiguration<AccountSetting>
{
    public void Configure(EntityTypeBuilder<AccountSetting> builder)
    {
        builder.ToTable("account_settings");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(a => a.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        builder.Property(a => a.Description).HasColumnName("description").HasMaxLength(200).IsRequired();

        builder.HasIndex(a => a.Code).IsUnique();

        builder.HasData(
            new AccountSetting { Id = 1,  Code = "151",  Description = "Hàng mua đang đi đường" },
            new AccountSetting { Id = 2,  Code = "152",  Description = "Nguyên liệu, vật liệu" },
            new AccountSetting { Id = 3,  Code = "1531", Description = "Công cụ, dụng cụ" },
            new AccountSetting { Id = 4,  Code = "1532", Description = "Bao bì luân chuyển" },
            new AccountSetting { Id = 5,  Code = "1533", Description = "Đồ dùng cho thuê" },
            new AccountSetting { Id = 6,  Code = "1534", Description = "Thiết bị, phụ tùng thay thế" },
            new AccountSetting { Id = 7,  Code = "1551", Description = "Thành phẩm nhập kho" },
            new AccountSetting { Id = 8,  Code = "1557", Description = "Thành phẩm bất động sản" },
            new AccountSetting { Id = 9,  Code = "1561", Description = "Giá mua hàng hóa" },
            new AccountSetting { Id = 10, Code = "1562", Description = "Chi phí thu mua hàng hóa" },
            new AccountSetting { Id = 11, Code = "1567", Description = "Hàng hóa bất động sản" },
            new AccountSetting { Id = 12, Code = "157",  Description = "Hàng gửi đi bán" },
            new AccountSetting { Id = 13, Code = "158",  Description = "Hàng hóa kho bảo thuế" },
            new AccountSetting { Id = 14, Code = "3339", Description = "Phí, lệ phí và các khoản phải nộp khác" },
            new AccountSetting { Id = 15, Code = "5111", Description = "Doanh thu bán hàng hóa" },
            new AccountSetting { Id = 16, Code = "5112", Description = "Doanh thu bán các thành phẩm" },
            new AccountSetting { Id = 17, Code = "5113", Description = "Doanh thu cung cấp dịch vụ" },
            new AccountSetting { Id = 18, Code = "5114", Description = "Doanh thu trợ cấp, trợ giá" },
            new AccountSetting { Id = 19, Code = "5117", Description = "Doanh thu kinh doanh bất động sản đầu tư" },
            new AccountSetting { Id = 20, Code = "5118", Description = "Doanh thu khác" },
            new AccountSetting { Id = 21, Code = "711",  Description = "Thu nhập khác" },
            new AccountSetting { Id = 22, Code = "154",  Description = "Chi phí sản xuất, kinh doanh dở dang" },
            new AccountSetting { Id = 23, Code = "2411", Description = "Mua sắm TSCĐ" },
            new AccountSetting { Id = 24, Code = "2412", Description = "Xây dựng cơ bản" },
            new AccountSetting { Id = 25, Code = "2413", Description = "Sửa chữa lớn TSCĐ" },
            new AccountSetting { Id = 26, Code = "242",  Description = "Chi phí trả trước" },
            new AccountSetting { Id = 27, Code = "6111", Description = "Mua nguyên liệu, vật liệu" },
            new AccountSetting { Id = 28, Code = "6112", Description = "Mua hàng hóa" },
            new AccountSetting { Id = 29, Code = "632",  Description = "Giá vốn hàng bán" },
            new AccountSetting { Id = 30, Code = "6232", Description = "Chi phí vật liệu" },
            new AccountSetting { Id = 31, Code = "6412", Description = "Chi phí vật liệu, bao bì" },
            new AccountSetting { Id = 32, Code = "6413", Description = "Chi phí dụng cụ, đồ dùng" },
            new AccountSetting { Id = 33, Code = "6417", Description = "Chi phí dịch vụ mua ngoài" },
            new AccountSetting { Id = 34, Code = "6422", Description = "Chi phí vật liệu quản lý" },
            new AccountSetting { Id = 35, Code = "6423", Description = "Chi phí đồ dùng văn phòng" },
            new AccountSetting { Id = 36, Code = "811",  Description = "Chi phí khác" },
            new AccountSetting { Id = 37, Code = "5211", Description = "Chiết khấu thương mại" },
            new AccountSetting { Id = 38, Code = "5212", Description = "Hàng bán bị trả lại" },
            new AccountSetting { Id = 39, Code = "5213", Description = "Giảm giá hàng bán" }
        );
    }
}
