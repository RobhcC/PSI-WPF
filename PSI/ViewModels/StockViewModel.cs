using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 库存查询页的 ViewModel。上下两个视图（主从）：
/// 上面是全部商品的库存结存，点选某一行，下面显示该商品的变动流水。
/// Include 预加载导航属性：一次查询把 Product 联表带出来，绑定 Product.Name 才有值。
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
                LoadLogs();
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

    public StockViewModel()
    {
        SearchCommand = new RelayCommand(_ => LoadStocks());

        LoadStocks();
    }

    public void LoadStocks()
    {
        using var db = new AppDbContext();

        var query = db.Stocks.Include(s => s.Product).AsQueryable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(s => s.Product!.Name.Contains(SearchText));
        }

        Stocks.Clear();
        foreach (var stock in query.OrderBy(s => s.ProductId).ToList())
        {
            Stocks.Add(stock);
        }

        // 重新加载后原来的选中对象已经不在列表里了，清掉选中并清空流水区
        SelectedStock = null;
    }

    /// <summary>加载选中商品的变动流水，按时间倒序（最近的在最上面）。</summary>
    public void LoadLogs()
    {
        Logs.Clear();
        if (SelectedStock == null)
        {
            return;
        }

        using var db = new AppDbContext();
        var logs = db.StockLogs
            .Include(l => l.Product)
            .Where(l => l.ProductId == SelectedStock.ProductId)
            .OrderByDescending(l => l.CreatedAt)
            .ToList();

        foreach (var log in logs)
        {
            Logs.Add(log);
        }
    }
}
