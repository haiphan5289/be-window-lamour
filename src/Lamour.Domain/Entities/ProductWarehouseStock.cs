namespace Lamour.Domain.Entities;

public class ProductWarehouseStock
{
    public int Id { get; set; }

    public int     ProductId { get; set; }
    public Product Product   { get; set; } = null!;

    public int       WarehouseId { get; set; }
    public Warehouse Warehouse   { get; set; } = null!;

    public int Quantity { get; set; }
}
