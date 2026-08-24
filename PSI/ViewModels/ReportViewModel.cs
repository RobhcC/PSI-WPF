using System.Collections.ObjectModel;
using PSI.Data;
using PSI.MVVM;

namespace PSI.ViewModels;

/// <summary>
/// 月度统计页的 ViewModel：按年查询，给出 12 个月的采购/销售汇总
/// （单据数 + 金额合计）和全年合计。
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

        // 采购：按月份分组统计单据数和金额（数据库端 GROUP BY）
        var purchase = db.PurchaseOrders
            .Where(o => o.OrderDate.Year == SelectedYear)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new MonthlyRow { Month = g.Key, OrderCount = g.Count(), Amount = g.Sum(x => x.TotalAmount) })
            .OrderBy(r => r.Month)
            .ToList();

        PurchaseRows.Clear();
        foreach (var row in purchase)
        {
            PurchaseRows.Add(row);
        }
        TotalPurchase = purchase.Sum(r => r.Amount);

        // 销售：同上
        var sale = db.SaleOrders
            .Where(o => o.OrderDate.Year == SelectedYear)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new MonthlyRow { Month = g.Key, OrderCount = g.Count(), Amount = g.Sum(x => x.TotalAmount) })
            .OrderBy(r => r.Month)
            .ToList();

        SaleRows.Clear();
        foreach (var row in sale)
        {
            SaleRows.Add(row);
        }
        TotalSale = sale.Sum(r => r.Amount);
    }
}
