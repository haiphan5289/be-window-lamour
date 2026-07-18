using Lamour.Domain.Enums;

namespace Lamour.Application.Features.Sales;

public static class SalesOrderTaxCalculator
{
    public static decimal ToPercent(VatRateType? vatRate) => vatRate switch
    {
        VatRateType.Five  => 5m,
        VatRateType.Eight => 8m,
        VatRateType.Ten   => 10m,
        _                 => 0m, // Zero, KCT, KKKNT, KHAC, null — không tính thuế
    };
}
