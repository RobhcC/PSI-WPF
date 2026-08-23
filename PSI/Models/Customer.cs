namespace PSI.Models;

/// <summary>
/// 客户。纯数据类，对应数据库 Customers 表。
/// 销售出库单引用它（卖给了谁）。字段和供应商一致，但业务角色不同，不合并成一张表。
/// </summary>
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string ContactPerson { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Address { get; set; } = "";
}
