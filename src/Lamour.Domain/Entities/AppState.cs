namespace Lamour.Domain.Entities;

// Bảng key/value nhỏ để lưu trạng thái nội bộ của ứng dụng (vd mốc thời gian lớn nhất từng thấy).
public class AppState
{
    public string   Key       { get; set; } = string.Empty;
    public string   Value     { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
