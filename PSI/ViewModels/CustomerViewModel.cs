using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;
using PSI.Windows;

namespace PSI.ViewModels;

/// <summary>客户列表页的 ViewModel，结构与供应商模块完全一致。</summary>
public class CustomerViewModel : ViewModelBase
{
    public ObservableCollection<Customer> Customers { get; } = new();

    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public CustomerViewModel()
    {
        SearchCommand = new RelayCommand(_ => LoadCustomers());
        AddCommand = new RelayCommand(_ => AddCustomer());
        EditCommand = new RelayCommand(_ => EditCustomer(), _ => SelectedCustomer != null);
        DeleteCommand = new RelayCommand(_ => DeleteCustomer(), _ => SelectedCustomer != null);

        LoadCustomers();
    }

    public void LoadCustomers()
    {
        using var db = new AppDbContext();

        var query = db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(c => c.Name.Contains(SearchText));
        }

        Customers.Clear();
        foreach (var customer in query.OrderBy(c => c.Id).ToList())
        {
            Customers.Add(customer);
        }
    }

    private void AddCustomer()
    {
        var editVm = new CustomerEditViewModel(null);
        var window = new CustomerEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = editVm,
        };

        if (window.ShowDialog() == true)
        {
            using var db = new AppDbContext();
            var customer = new Customer();
            editVm.ApplyTo(customer);
            db.Customers.Add(customer);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // 数据库拒绝写入（如名称超过 50 字上限）：提示原因，放弃本次保存
                MessageBox.Show(
                    $"保存失败：{ex.InnerException?.Message ?? ex.Message}",
                    "保存失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            LoadCustomers();
        }
    }

    private void EditCustomer()
    {
        var editVm = new CustomerEditViewModel(SelectedCustomer!);
        var window = new CustomerEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = editVm,
        };

        if (window.ShowDialog() == true)
        {
            using var db = new AppDbContext();
            var customer = db.Customers.Find(SelectedCustomer!.Id);
            if (customer != null)
            {
                editVm.ApplyTo(customer);
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    MessageBox.Show(
                        $"保存失败：{ex.InnerException?.Message ?? ex.Message}",
                        "保存失败",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }
            LoadCustomers();
        }
    }

    private void DeleteCustomer()
    {
        var customer = SelectedCustomer!;

        var answer = MessageBox.Show(
            $"确定删除客户「{customer.Name}」吗？",
            "确认删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        using var db = new AppDbContext();
        try
        {
            var toDelete = db.Customers.Find(customer.Id);
            if (toDelete != null)
            {
                db.Customers.Remove(toDelete);
                db.SaveChanges();
            }
            LoadCustomers();
        }
        catch (DbUpdateException)
        {
            // 客户被销售单引用时，数据库 Restrict 约束拒绝删除
            MessageBox.Show(
                "该客户已被销售单据引用，不能删除。",
                "无法删除",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
