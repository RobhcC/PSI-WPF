namespace PSI.Models;

/// <summary>
/// 销售出库单（单据头），结构与采购单对称，区别是关联客户、保存后扣库存。
/// </summary>
public class SaleOrder
{
    public int Id { get; set; }

    /// <summary>单据编号，如 XS20260824001。</summary>
    public string OrderNo { get; set; } = "";

    /// <summary>客户外键：这张单卖给了谁。</summary>
    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public Customer Customer { get; set; } = null!;

    public List<SaleOrderDetail> Details { get; set; } = new();
}
