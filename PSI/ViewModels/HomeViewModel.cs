using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;

namespace PSI.ViewModels;

/// <summary>
/// 首页 ViewModel：打开程序第一眼的"经营概况板"。
/// 六张概览卡片（档案数、库存占用资金、今年单据与销售额）+ 两张迷你榜
/// （低库存预警、畅销商品）。全部用 Count/Sum/GroupBy 在数据库端一次算完，
/// 首页不做定时刷新——单据页才是干活的地方，首页只给概况。
/// </summary>
public class HomeViewModel : ViewModelBase
{
    /// <summary>低库存榜的一行：商品名 + 当前结存。</summary>
    public class LowStockRow
    {
        public string ProductName { get; init; } = "";

        public int Quantity { get; init; }
    }

    /// <summary>畅销榜的一行：商品名 + 销售数量 + 销售金额。</summary>
    public class HotProductRow
    {
        public string ProductName { get; init; } = "";

        public int Quantity { get; init; }

        public decimal Amount { get; init; }
    }

    private int _productCount;
    public int ProductCount
    {
        get => _productCount;
        private set => SetProperty(ref _productCount, value);
    }

    private int _supplierCount;
    public int SupplierCount
    {
        get => _supplierCount;
        private set => SetProperty(ref _supplierCount, value);
    }

    private int _customerCount;
    public int CustomerCount
    {
        get => _customerCount;
        private set => SetProperty(ref _customerCount, value);
    }

    private decimal _stockValue;
    public decimal StockValue
    {
        get => _stockValue;
        private set => SetProperty(ref _stockValue, value);
    }

    private int _purchaseOrderCount;
    public int PurchaseOrderCount
    {
        get => _purchaseOrderCount;
        private set => SetProperty(ref _purchaseOrderCount, value);
    }

    private decimal _saleAmount;
    public decimal SaleAmount
    {
        get => _saleAmount;
        private set => SetProperty(ref _saleAmount, value);
    }

    /// <summary>低库存 TOP 5（结存最少的前五，演示数据里天然有"快卖光"的商品）。</summary>
    public ObservableCollection<LowStockRow> LowStockItems { get; } = new();

    /// <summary>畅销商品 TOP 5（按销售金额，与月度统计页口径一致）。</summary>
    public ObservableCollection<HotProductRow> HotProducts { get; } = new();

    private int _statYear;
    /// <summary>当前年份，卡片文案显示"今年"用。</summary>
    public int StatYear
    {
        get => _statYear;
        private set => SetProperty(ref _statYear, value);
    }

    public HomeViewModel()
    {
        StatYear = DateTime.Now.Year;

        using var db = new AppDbContext();

        // 三张基础档案只数数量，Count 翻译成 SQL 的 COUNT(*)，不拉数据回来
        ProductCount = db.Products.Count();
        SupplierCount = db.Suppliers.Count();
        CustomerCount = db.Customers.Count();

        // 库存占用资金 = Σ(结存数量 × 商品采购价)：库存联商品表，数据库端一次聚合算完。
        // 用采购价是保守口径（还没卖掉的都是成本投入），销售口径要按售价另算
        StockValue = db.Stocks.Sum(s => s.Quantity * s.Product.PurchasePrice);

        PurchaseOrderCount = db.PurchaseOrders.Count(o => o.OrderDate.Year == StatYear);
        SaleAmount = db.SaleOrders
            .Where(o => o.OrderDate.Year == StatYear)
            .Sum(o => o.TotalAmount);

        // 低库存预警：结存升序取 5 条，提示"该补货了"
        LowStockItems.Clear();
        var low = db.Stocks
            .Include(s => s.Product)
            .OrderBy(s => s.Quantity)
            .Take(5)
            .ToList();
        foreach (var s in low)
        {
            LowStockItems.Add(new LowStockRow { ProductName = s.Product.Name, Quantity = s.Quantity });
        }

        // 畅销榜：按销售金额 GroupBy 取前五，与报表页同一个写法（数据库端 GROUP BY + TOP）
        HotProducts.Clear();
        var hot = db.SaleOrderDetails
            .Where(d => d.SaleOrder.OrderDate.Year == StatYear)
            .GroupBy(d => new { d.ProductId, d.Product.Name })
            .Select(g => new HotProductRow { ProductName = g.Key.Name, Quantity = g.Sum(x => x.Quantity), Amount = g.Sum(x => x.Amount) })
            .OrderByDescending(r => r.Amount)
            .Take(5)
            .ToList();
        foreach (var row in hot)
        {
            HotProducts.Add(row);
        }
    }
}
