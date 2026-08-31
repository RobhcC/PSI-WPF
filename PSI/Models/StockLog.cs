namespace PSI.Models;

/// <summary>
/// 库存变动流水：每次入库/出库记一行，只增不改，作审计记录。
/// OrderNo 只存单号不建外键，历史流水不随单据变动而改变。
/// </summary>
public class StockLog
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    /// <summary>变动类型：采购入库 / 销售出库。</summary>
    public string ChangeType { get; set; } = "";

    /// <summary>变动数量（正数，方向由类型决定）。</summary>
    public int Quantity { get; set; }

    public string OrderNo { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public Product Product { get; set; } = null!;
}
