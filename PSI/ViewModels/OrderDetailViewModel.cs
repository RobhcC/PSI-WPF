using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 单据明细只读查看的 ViewModel，采购/销售共用。
/// 构造时按 Id 用新 DbContext 重查明细，不碰列表页传来的旧对象导航属性。
/// </summary>
public class OrderDetailViewModel
{
    /// <summary>明细表格的一行。</summary>
    public class Row
    {
        public string ProductName { get; init; } = "";

        public int Quantity { get; init; }

        public decimal UnitPrice { get; init; }

        public decimal Amount { get; init; }
    }

    /// <summary>窗口标题：区分采购/销售。</summary>
    public string Title { get; }

    /// <summary>往来单位字段名："供应商"或"客户"。</summary>
    public string PartnerLabel { get; }

    /// <summary>往来单位名称。</summary>
    public string PartnerName { get; }

    public string OrderNo { get; }

    public DateTime OrderDate { get; }

    public decimal TotalAmount { get; }

    public ObservableCollection<Row> Rows { get; } = new();

    public OrderDetailViewModel(PurchaseOrder order)
        : this("采购入库单明细", "供应商", order.Supplier.Name, order.OrderNo, order.OrderDate, order.TotalAmount)
    {
        using var db = new AppDbContext();
        var rows = db.PurchaseOrderDetails
            .Include(d => d.Product)
            .Where(d => d.PurchaseOrderId == order.Id)
            .OrderBy(d => d.Id)
            .Select(d => new Row { ProductName = d.Product.Name, Quantity = d.Quantity, UnitPrice = d.UnitPrice, Amount = d.Amount })
            .ToList();
        foreach (var row in rows)
        {
            Rows.Add(row);
        }
    }

    public OrderDetailViewModel(SaleOrder order)
        : this("销售出库单明细", "客户", order.Customer.Name, order.OrderNo, order.OrderDate, order.TotalAmount)
    {
        using var db = new AppDbContext();
        var rows = db.SaleOrderDetails
            .Include(d => d.Product)
            .Where(d => d.SaleOrderId == order.Id)
            .OrderBy(d => d.Id)
            .Select(d => new Row { ProductName = d.Product.Name, Quantity = d.Quantity, UnitPrice = d.UnitPrice, Amount = d.Amount })
            .ToList();
        foreach (var row in rows)
        {
            Rows.Add(row);
        }
    }

    /// <summary>公共字段初始化，两个业务构造函数共用。</summary>
    private OrderDetailViewModel(string title, string partnerLabel, string partnerName, string orderNo, DateTime orderDate, decimal totalAmount)
    {
        Title = title;
        PartnerLabel = partnerLabel;
        PartnerName = partnerName;
        OrderNo = orderNo;
        OrderDate = orderDate;
        TotalAmount = totalAmount;
    }
}
