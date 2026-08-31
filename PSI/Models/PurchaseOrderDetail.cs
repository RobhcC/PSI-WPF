namespace PSI.Models;

/// <summary>
/// 采购入库单明细行：一行记录买了哪个商品、数量、单价、金额。
/// </summary>
public class PurchaseOrderDetail
{
    public int Id { get; set; }

    /// <summary>所属单据头外键。</summary>
    public int PurchaseOrderId { get; set; }

    /// <summary>商品外键。</summary>
    public int ProductId { get; set; }

    /// <summary>数量。</summary>
    public int Quantity { get; set; }

    /// <summary>成交单价，按单记实价，可能与商品默认采购价不同。</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>金额 = 数量 × 单价（保存时由程序计算）。</summary>
    public decimal Amount { get; set; }

    /// <summary>导航属性。</summary>
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    /// <summary>导航属性。</summary>
    public Product Product { get; set; } = null!;
}
