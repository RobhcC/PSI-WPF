namespace PSI.Models;

/// <summary>
/// 商品。纯数据类（POCO）：只描述一行数据的形状，不带任何界面逻辑。
/// 对应数据库 Products 表。
/// </summary>
public class Product
{
    public int Id { get; set; }

    /// <summary>商品名称。</summary>
    public string Name { get; set; } = "";

    /// <summary>分类（如：电阻、传感器），用字符串保持简单。</summary>
    public string Category { get; set; } = "";

    /// <summary>计量单位（如：个、箱）。</summary>
    public string Unit { get; set; } = "";

    /// <summary>采购价（进价）。</summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>销售价（售价），必须高于采购价才有利润。</summary>
    public decimal SalePrice { get; set; }
}
