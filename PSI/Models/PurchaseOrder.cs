namespace PSI.Models;

/// <summary>
/// 采购入库单（单据头）。主从表的"主"：一张单据记录"什么时候、从哪个供应商、总共多少钱"。
/// 具体买了哪些商品，每样买多少，记录在明细表 PurchaseOrderDetail（从表）里。
/// </summary>
public class PurchaseOrder
{
    public int Id { get; set; }

    /// <summary>单据编号，如 CG20260824001，业务上人工可读，建了唯一索引。</summary>
    public string OrderNo { get; set; } = "";

    /// <summary>供应商外键：这张单从谁那里进货。</summary>
    public int SupplierId { get; set; }

    /// <summary>单据日期。</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>单据总金额 = 所有明细行金额之和（保存时由程序计算）。</summary>
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>导航属性：通过它可以拿到供应商对象，界面显示供应商名称时用。
    /// EF 会根据 SupplierId 自动填它（联表查询）。</summary>
    public Supplier Supplier { get; set; } = null!;

    /// <summary>导航属性：这张单的所有明细行。</summary>
    public List<PurchaseOrderDetail> Details { get; set; } = new();
}
