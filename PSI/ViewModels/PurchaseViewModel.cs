using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;
using PSI.Windows;

namespace PSI.ViewModels;

/// <summary>
/// 采购单列表页的 ViewModel。
/// 刻意不提供删除：单据保存时已联动库存，直接删单会造成账实不符。
/// 真实 ERP 的做法是"红字冲销"（开一张负数单抵消），本项目规模下选择禁止删除，
/// 数据完整性优先——这也是一个面试可讲的设计取舍。
/// </summary>
public class PurchaseViewModel : ViewModelBase
{
    public ObservableCollection<PurchaseOrder> Orders { get; } = new();

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand AddCommand { get; }

    public PurchaseViewModel()
    {
        SearchCommand = new RelayCommand(_ => LoadOrders());
        AddCommand = new RelayCommand(_ => AddOrder());

        LoadOrders();
    }

    public void LoadOrders()
    {
        using var db = new AppDbContext();

        var query = db.PurchaseOrders.Include(o => o.Supplier).AsQueryable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(o => o.OrderNo.Contains(SearchText));
        }

        Orders.Clear();
        foreach (var order in query.OrderByDescending(o => o.Id).ToList())
        {
            Orders.Add(order);
        }
    }

    private void AddOrder()
    {
        var editVm = new PurchaseEditViewModel();
        var window = new PurchaseEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = editVm,
        };

        if (window.ShowDialog() == true)
        {
            LoadOrders();
        }
    }
}
