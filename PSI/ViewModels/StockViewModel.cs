using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 库存查询页的 ViewModel：上表库存结存，选中一行下方显示该商品变动流水。
/// 查询在后台线程执行，避免卡住界面。
/// </summary>
public class StockViewModel : ViewModelBase
{
    public ObservableCollection<Stock> Stocks { get; } = new();

    public ObservableCollection<StockLog> Logs { get; } = new();

    private Stock? _selectedStock;

    /// <summary>当前选中的库存行，变化时重查该商品流水。</summary>
    public Stock? SelectedStock
    {
        get => _selectedStock;
        set
        {
            if (SetProperty(ref _selectedStock, value))
            {
                _ = LoadLogsAsync();
            }
        }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public RelayCommand SearchCommand { get; }

    /// <summary>过期结果保护：只允许最新一次加载的结果写回界面，
    /// 防止连续操作时旧查询后到覆盖新数据。</summary>
    private int _stockVersion;
    private int _logVersion;

    public StockViewModel()
    {
        SearchCommand = new RelayCommand(_ => { _ = LoadStocksAsync(); });

        _ = LoadStocksAsync();
    }

    /// <summary>
    /// 异步加载库存列表。查询关在 Task.Run 里用局部 DbContext（非线程安全），
    /// await 后回到 UI 线程才能更新绑定集合。
    /// </summary>
    private async Task LoadStocksAsync()
    {
        var keyword = SearchText;
        var version = ++_stockVersion;

        try
        {
            var stocks = await Task.Run(() =>
            {
                using var db = new AppDbContext();

                var query = db.Stocks.Include(s => s.Product).AsQueryable();
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(s => s.Product!.Name.Contains(keyword));
                }

                return query.OrderBy(s => s.ProductId).ToList();
            });

            if (version != _stockVersion)
            {
                return; // 期间用户又点了查询，这次的结果已过期，丢弃
            }

            Stocks.Clear();
            foreach (var stock in stocks)
            {
                Stocks.Add(stock);
            }

            // 重新加载后原来的选中对象已经不在列表里了，清掉选中并清空流水区
            SelectedStock = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"库存加载失败：{ex.Message}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>加载选中商品的变动流水，按时间倒序。</summary>
    private async Task LoadLogsAsync()
    {
        Logs.Clear();
        var stock = SelectedStock;
        if (stock == null)
        {
            return;
        }

        var version = ++_logVersion;

        try
        {
            var logs = await Task.Run(() =>
            {
                using var db = new AppDbContext();
                return db.StockLogs
                    .Include(l => l.Product)
                    .Where(l => l.ProductId == stock.ProductId)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToList();
            });

            if (version != _logVersion)
            {
                return; // 用户已换选了别的行，这次的结果已过期，丢弃
            }

            foreach (var log in logs)
            {
                Logs.Add(log);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"流水加载失败：{ex.Message}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
