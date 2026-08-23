namespace PSI.Models;

/// <summary>
/// 采购入库单明细行（从表）。主从表的"从"：一行记录"买了哪个商品、数量、单价、金额"。
/// 为什么单独建表：一张单的商品行数不确定，一行数据必须能独立存一行，这就是"关系型"的含义。
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

    /// <summary>成交单价（可能和商品的默认采购价不同，按单记实价）。</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>金额 = 数量 × 单价（保存时由程序计算）。</summary>
    public decimal Amount { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    /// <summary>导航属性：界面显示商品名称时用。</summary>
    public Product Product { get; set; } = null!;
}
