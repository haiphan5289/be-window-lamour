namespace Lamour.Domain.Enums;

/// <summary>
/// Entry point a new SalesOrder is created from — determines the DocumentNumber prefix.
/// WarehouseExport = "XK", Direct = "BH". Two independently counted sequences; every other
/// business rule (stock deduction, VAT, reporting) is identical regardless of source.
/// </summary>
public enum SalesOrderCodeSource
{
    WarehouseExport,
    Direct
}
