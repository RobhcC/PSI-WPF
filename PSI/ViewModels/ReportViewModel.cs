using System.Collections.ObjectModel;
using PSI.Data;
using PSI.MVVM;

namespace PSI.ViewModels;

/// <summary>
/// 月度统计页的 ViewModel：按年查询，给出 12 个月的采购/销售汇总
/// （单据数 + 金额合计 + 销售毛利估算）、畅销商品 TOP 5 和全年合计。
/// GroupBy 在数据库端执行（翻译成 SQL 的 GROUP BY），不是把全年数据拉到内存再算——
/// 数据量小的时候看不出差别，但写法本身是对的。
/// </summary>
public class ReportViewModel : ViewModelBase
{
    /// <summary>汇总表的一行：某个月的统计值。只用于展示，不需要通知机制。</summary>
    public class MonthlyRow
    {
        public int Month { get; init; }

        public int OrderCount { get; init; }

        public decimal Amount { get; init; }

        /// <summary>毛利估算（只有销售表用得上）：本月销售的（售价-采购价）×数量合计。</summary>
        public decimal Profit { get; init; }
    }

    /// <summary>畅销商品排行的一行：某商品全年的销量与销售额合计。</summary>
    public class TopProductRow
    {
        public string ProductName { get; init; } = "";

        public int Quantity { get; init; }

        public decimal Amount { get; init; }
    }

    public ObservableCollection<int> Years { get; } = new();

    private int _selectedYear;
    public int SelectedYear
    {
        get => _selectedYear;
        set => SetProperty(ref _selectedYear, value);
    }

    public ObservableCollection<MonthlyRow> PurchaseRows { get; } = new();
    public ObservableCollection<MonthlyRow> SaleRows { get; } = new();

    /// <summary>畅销商品 TOP 5（按销售金额排序）。</summary>
    public ObservableCollection<TopProductRow> TopProducts { get; } = new();

    private decimal _totalPurchase;
    public decimal TotalPurchase
    {
        get => _totalPurchase;
        private set => SetProperty(ref _totalPurchase, value);
    }

    private decimal _totalSale;
    public decimal TotalSale
    {
        get => _totalSale;
        private set => SetProperty(ref _totalSale, value);
    }

    private decimal _totalProfit;
    public decimal TotalProfit
    {
        get => _totalProfit;
        private set => SetProperty(ref _totalProfit, value);
    }

    public RelayCommand QueryCommand { get; }

    public ReportViewModel()
    {
        // 提供最近三个年份可选，默认当年
        for (int year = DateTime.Now.Year; year >= DateTime.Now.Year - 2; year--)
        {
            Years.Add(year);
        }
        _selectedYear = DateTime.Now.Year;

        QueryCommand = new RelayCommand(_ => LoadReport());

        LoadReport();
    }

    public void LoadReport()
    {
        using var db = new AppDbContext();

        // 采购：按月份分组统计单据数和金额（数据库端 GROUP BY）。
        // 结果先收进字典，下面按 1~12 月补齐时按月取
        var purchaseByMonth = db.PurchaseOrders
            .Where(o => o.OrderDate.Year == SelectedYear)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new { Month = g.Key, Count = g.Count(), Amount = g.Sum(x => x.TotalAmount) })
            .ToDictionary(x => x.Month);

        // 销售：同上
        var saleByMonth = db.SaleOrders
            .Where(o => o.OrderDate.Year == SelectedYear)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new { Month = g.Key, Count = g.Count(), Amount = g.Sum(x => x.TotalAmount) })
            .ToDictionary(x => x.Month);

        // 毛利估算：从销售明细联商品表，(成交单价 - 商品采购价) × 数量，按月合计。
        // 成本取商品档案的当前采购价，是估算口径——真实成本核算要按批次加权平均，
        // 那是完整 ERP 的功能，这里给个够用的管理视角
        var profitByMonth = db.SaleOrderDetails
            .Where(d => d.SaleOrder.OrderDate.Year == SelectedYear)
            .GroupBy(d => d.SaleOrder.OrderDate.Month)
            .Select(g => new { Month = g.Key, Profit = g.Sum(x => (x.UnitPrice - x.Product.PurchasePrice) * x.Quantity) })
            .ToDictionary(x => x.Month);

        // 固定显示 1~12 月：GroupBy 只返回有单据的月份，没数据的月份补 0，
        // 报表一眼看全年，不用脑补缺了哪几个月
        PurchaseRows.Clear();
        SaleRows.Clear();
        for (int month = 1; month <= 12; month++)
        {
            var p = purchaseByMonth.GetValueOrDefault(month);
            PurchaseRows.Add(new MonthlyRow { Month = month, OrderCount = p?.Count ?? 0, Amount = p?.Amount ?? 0 });

            var s = saleByMonth.GetValueOrDefault(month);
            var f = profitByMonth.GetValueOrDefault(month);
            SaleRows.Add(new MonthlyRow { Month = month, OrderCount = s?.Count ?? 0, Amount = s?.Amount ?? 0, Profit = f?.Profit ?? 0 });
        }

        TotalPurchase = PurchaseRows.Sum(r => r.Amount);
        TotalSale = SaleRows.Sum(r => r.Amount);
        TotalProfit = SaleRows.Sum(r => r.Profit);

        // 畅销商品 TOP 5：按销售金额取前五，GroupBy + Take(5) 同样在数据库端完成
        // （翻译成 GROUP BY + TOP(5)），不把全年明细拉回内存排序
        TopProducts.Clear();
        var top = db.SaleOrderDetails
            .Where(d => d.SaleOrder.OrderDate.Year == SelectedYear)
            .GroupBy(d => new { d.ProductId, d.Product.Name })
            .Select(g => new TopProductRow { ProductName = g.Key.Name, Quantity = g.Sum(x => x.Quantity), Amount = g.Sum(x => x.Amount) })
            .OrderByDescending(r => r.Amount)
            .Take(5)
            .ToList();
        foreach (var row in top)
        {
            TopProducts.Add(row);
        }
    }
}
