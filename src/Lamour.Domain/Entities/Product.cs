using Lamour.Domain.Enums;


namespace Lamour.Domain.Entities;

public class Product
{
    public int          Id                { get; set; }
    public string       Code              { get; set; } = string.Empty;
    public string       Name              { get; set; } = string.Empty;
    public int          CategoryId        { get; set; }
    public Category     Category          { get; set; } = null!;
    public string       Unit              { get; set; } = string.Empty;
    public decimal      CostPrice         { get; set; }
    public decimal      SellingPrice      { get; set; }
    public int          StockQuantity     { get; set; }
    public bool         IsActive          { get; set; } = true;

    // Tax fields
    public VatRateType? VatRate           { get; set; }
    public TaxReductionStatus? TaxReductionType  { get; set; }
    public decimal?     ImportTaxRate     { get; set; }
    public decimal?     ExportTaxRate     { get; set; }
    public string?      ExciseTaxGroup    { get; set; }
}
