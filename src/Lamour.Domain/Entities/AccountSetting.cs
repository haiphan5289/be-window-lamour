namespace Lamour.Domain.Entities;

public class AccountSetting
{
    public int    Id          { get; set; }
    public string Code        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
