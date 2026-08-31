using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;
using PSI.Windows;

namespace PSI.ViewModels;

/// <summary>
/// 采购单列表页的 ViewModel。单据保存时已联动库存，不提供删除，避免账实不符。
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
    public RelayCommand ViewCommand { get; }

    private PurchaseOrder? _selectedOrder;

    /// <summary>列表当前选中的单据。查看明细命令的可用条件（选中才亮）。</summary>
    public PurchaseOrder? SelectedOrder
    {
        get => _selectedOrder;
        set => SetProperty(ref _selectedOrder, value);
    }

    public PurchaseViewModel()
    {
        SearchCommand = new RelayCommand(_ => LoadOrders());
        AddCommand = new RelayCommand(_ => AddOrder());
        ViewCommand = new RelayCommand(_ => ViewOrder(), _ => SelectedOrder != null);

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

    /// <summary>只读查看选中单据的明细：弹窗显示单头信息 + 明细行，不可编辑。</summary>
    private void ViewOrder()
    {
        if (SelectedOrder == null)
        {
            return;
        }

        var detailVm = new OrderDetailViewModel(SelectedOrder);
        var window = new OrderDetailWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = detailVm,
        };

        window.ShowDialog();
    }
}
