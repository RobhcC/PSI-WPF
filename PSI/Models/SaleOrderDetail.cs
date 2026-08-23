namespace PSI.Models;

/// <summary>销售出库单明细行（从表）。</summary>
public class SaleOrderDetail
{
    public int Id { get; set; }

    public int SaleOrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public SaleOrder SaleOrder { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
