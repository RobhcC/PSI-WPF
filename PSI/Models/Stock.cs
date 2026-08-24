namespace PSI.Models;

/// <summary>
/// 库存表：每个商品的当前结存数量。
/// 为什么单独建表而不是每次用"采购合计-销售合计"现算：
/// ① 查询快且语义清晰（库存是"状态"，不是每次重算的"结果"）；
/// ② 单据保存时在同一事务里直接加减，账实一致的责任边界清楚。
/// ProductId 建唯一索引：一个商品只有一行库存。
/// </summary>
public class Stock
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    /// <summary>当前结存数量。采购入库 +，销售出库 -。</summary>
    public int Quantity { get; set; }

    public Product Product { get; set; } = null!;
}
