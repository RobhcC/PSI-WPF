using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;

namespace PSI.ViewModels;

/// <summary>
/// 首页 ViewModel：经营概况卡片 + 低库存预警 + 畅销榜，统计在数据库端聚合。
/// 加载在后台线程执行，窗口先显示，数据到位后填充。
/// </summary>
public class HomeViewModel : ViewModelBase
{
    /// <summary>低库存榜的一行。</summary>
    public class LowStockRow
    {
        public string ProductName { get; init; } = "";

        public int Quantity { get; init; }
    }

    /// <summary>畅销榜的一行。</summary>
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

    /// <summary>低库存 TOP 5。</summary>
    public ObservableCollection<LowStockRow> LowStockItems { get; } = new();

    /// <summary>畅销商品 TOP 5。</summary>
    public ObservableCollection<HotProductRow> HotProducts { get; } = new();

    private int _statYear;
    /// <summary>当前统计年份。</summary>
    public int StatYear
    {
        get => _statYear;
        private set => SetProperty(ref _statYear, value);
    }

    public HomeViewModel()
    {
        StatYear = DateTime.Now.Year;

        _ = LoadAsync();
    }

    /// <summary>
    /// 后台线程一次算完所有指标，回 UI 线程后赋值属性、填充集合。
    /// </summary>
    private async Task LoadAsync()
    {
        var year = StatYear;

        try
        {
            var result = await Task.Run(() =>
            {
                using var db = new AppDbContext();

                // 三张基础档案只数数量，Count 翻译成 SQL 的 COUNT(*)，不拉数据回来
                var productCount = db.Products.Count();
                var supplierCount = db.Suppliers.Count();
                var customerCount = db.Customers.Count();

                // 库存占用资金 = Σ(结存数量 × 商品采购价)，按成本口径
                var stockValue = db.Stocks.Sum(s => s.Quantity * s.Product.PurchasePrice);

                var purchaseOrderCount = db.PurchaseOrders.Count(o => o.OrderDate.Year == year);
                var saleAmount = db.SaleOrders
                    .Where(o => o.OrderDate.Year == year)
                    .Sum(o => o.TotalAmount);

                // 低库存预警 TOP 5
                var low = db.Stocks
                    .Include(s => s.Product)
                    .OrderBy(s => s.Quantity)
                    .Take(5)
                    .Select(s => new LowStockRow { ProductName = s.Product.Name, Quantity = s.Quantity })
                    .ToList();

                // 畅销榜 TOP 5，按销售金额
                var hot = db.SaleOrderDetails
                    .Where(d => d.SaleOrder.OrderDate.Year == year)
                    .GroupBy(d => new { d.ProductId, d.Product.Name })
                    .Select(g => new HotProductRow { ProductName = g.Key.Name, Quantity = g.Sum(x => x.Quantity), Amount = g.Sum(x => x.Amount) })
                    .OrderByDescending(r => r.Amount)
                    .Take(5)
                    .ToList();

                return (productCount, supplierCount, customerCount, stockValue,
                    purchaseOrderCount, saleAmount, low, hot);
            });

            ProductCount = result.productCount;
            SupplierCount = result.supplierCount;
            CustomerCount = result.customerCount;
            StockValue = result.stockValue;
            PurchaseOrderCount = result.purchaseOrderCount;
            SaleAmount = result.saleAmount;

            LowStockItems.Clear();
            foreach (var s in result.low)
            {
                LowStockItems.Add(s);
            }

            HotProducts.Clear();
            foreach (var row in result.hot)
            {
                HotProducts.Add(row);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"首页数据加载失败：{ex.Message}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
