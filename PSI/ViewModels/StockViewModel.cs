using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 库存查询页的 ViewModel。上下两个视图（主从）：
/// 上面是全部商品的库存结存，点选某一行，下面显示该商品的变动流水。
/// Include 预加载导航属性：一次查询把 Product 联表带出来，绑定 Product.Name 才有值。
/// 查询全部走后台线程：EF 首次编译查询 + LocalDB 启动都要几百毫秒到一秒多，
/// 若在 UI 线程同步查，点菜单的瞬间整个界面会冻住（卡顿的根源）。
/// </summary>
public class StockViewModel : ViewModelBase
{
    public ObservableCollection<Stock> Stocks { get; } = new();

    public ObservableCollection<StockLog> Logs { get; } = new();

    private Stock? _selectedStock;

    /// <summary>当前选中的库存行。一变就重查该商品的流水（主从联动）。</summary>
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

    /// <summary>过期结果保护：每次发起新加载就自增。只有最新一次的结果才允许写回界面，
    /// 防止"快速换行选中"时先发的旧查询后到，把新流水覆盖成旧数据。</summary>
    private int _stockVersion;
    private int _logVersion;

    public StockViewModel()
    {
        SearchCommand = new RelayCommand(_ => { _ = LoadStocksAsync(); });

        _ = LoadStocksAsync();
    }

    /// <summary>
    /// 异步加载库存列表。DbContext 不是线程安全的：查询和 Dispose 全部关在 Task.Run 里，
    /// 用自己的局部 db，不跨线程共享；await 之后回到 UI 线程，才能动 ObservableCollection
    /// （绑定它的 DataGrid 只认 UI 线程的修改）。
    /// </summary>
    private async Task LoadStocksAsync()
    {
        var keyword = SearchText; // 先在 UI 线程取值再进后台，避免后台读绑定属性
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

    /// <summary>加载选中商品的变动流水，按时间倒序（最近的在最上面）。同样走后台线程。</summary>
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
