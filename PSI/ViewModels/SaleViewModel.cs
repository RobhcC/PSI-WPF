using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;
using PSI.Windows;

namespace PSI.ViewModels;

/// <summary>销售单列表页的 ViewModel。与采购单列表同构，同样不提供删除（保护账实一致）。</summary>
public class SaleViewModel : ViewModelBase
{
    public ObservableCollection<SaleOrder> Orders { get; } = new();

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand AddCommand { get; }

    public SaleViewModel()
    {
        SearchCommand = new RelayCommand(_ => LoadOrders());
        AddCommand = new RelayCommand(_ => AddOrder());

        LoadOrders();
    }

    public void LoadOrders()
    {
        using var db = new AppDbContext();

        var query = db.SaleOrders.Include(o => o.Customer).AsQueryable();
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
        var editVm = new SaleEditViewModel();
        var window = new SaleEditWindow
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
