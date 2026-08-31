namespace PSI.Models;

/// <summary>
/// 库存表：每个商品的当前结存。单独建状态表而不是用采购/销售差额现算，
/// 单据保存时在同一事务内加减，保证账实一致。
/// </summary>
public class Stock
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    /// <summary>当前结存数量。采购入库 +，销售出库 -。</summary>
    public int Quantity { get; set; }

    public Product Product { get; set; } = null!;
}
