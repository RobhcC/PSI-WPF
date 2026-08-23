namespace PSI.Models;

/// <summary>
/// 供应商。纯数据类，对应数据库 Suppliers 表。
/// 采购入库单引用它（从谁那里买的）。
/// </summary>
public class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    /// <summary>联系人。</summary>
    public string ContactPerson { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Address { get; set; } = "";
}
