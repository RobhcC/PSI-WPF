namespace PSI.Models;

/// <summary>
/// 客户，销售出库单引用。字段与供应商一致但业务角色不同，不合并成一张表。
/// </summary>
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string ContactPerson { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Address { get; set; } = "";
}
