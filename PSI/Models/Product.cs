namespace PSI.Models;

/// <summary>
/// 商品，对应数据库 Products 表。纯 POCO，不含界面逻辑。
/// </summary>
public class Product
{
    public int Id { get; set; }

    /// <summary>商品名称。</summary>
    public string Name { get; set; } = "";

    /// <summary>分类。</summary>
    public string Category { get; set; } = "";

    /// <summary>计量单位（如：个、箱）。</summary>
    public string Unit { get; set; } = "";

    /// <summary>采购价（进价）。</summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>销售价。</summary>
    public decimal SalePrice { get; set; }
}
