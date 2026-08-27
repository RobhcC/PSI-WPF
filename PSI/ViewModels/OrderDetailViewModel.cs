using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 单据明细只读查看的 ViewModel。采购单和销售单共用一套：
/// 展示形状完全相同（单头信息 + 商品/数量/单价/金额的明细行），
/// 只读无编辑逻辑，分开写两套纯属重复。构造时用新的 DbContext
/// 按 Id 重查明细（列表页传进来的 order 来自已释放的 context，
/// 只读它的标量值不碰导航属性，不会引发跟踪冲突）。
/// </summary>
public class OrderDetailViewModel
{
    /// <summary>明细表格的一行：商品名 + 数量 + 单价 + 金额，纯展示。</summary>
    public class Row
    {
        public string ProductName { get; init; } = "";

        public int Quantity { get; init; }

        public decimal UnitPrice { get; init; }

        public decimal Amount { get; init; }
    }

    /// <summary>窗口标题：区分采购/销售。</summary>
    public string Title { get; }

    /// <summary>往来单位的字段名："供应商"或"客户"（界面标签跟着变）。</summary>
    public string PartnerLabel { get; }

    /// <summary>往来单位名称（供应商名或客户名）。</summary>
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

    /// <summary>公共字段初始化，两个业务构造函数共用。partnerName 由列表页的
    /// Include(o => o.Supplier/Customer) 预加载好了，直接读导航属性即可。</summary>
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
