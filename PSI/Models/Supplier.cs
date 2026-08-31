namespace PSI.Models;

/// <summary>
/// 供应商，采购入库单引用。
/// </summary>
public class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string ContactPerson { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Address { get; set; } = "";
}
