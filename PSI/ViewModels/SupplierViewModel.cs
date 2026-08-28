using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;
using PSI.Windows;

namespace PSI.ViewModels;

/// <summary>供应商列表页的 ViewModel，结构与商品模块完全一致。</summary>
public class SupplierViewModel : ViewModelBase
{
    public ObservableCollection<Supplier> Suppliers { get; } = new();

    private Supplier? _selectedSupplier;
    public Supplier? SelectedSupplier
    {
        get => _selectedSupplier;
        set => SetProperty(ref _selectedSupplier, value);
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

    public SupplierViewModel()
    {
        SearchCommand = new RelayCommand(_ => LoadSuppliers());
        AddCommand = new RelayCommand(_ => AddSupplier());
        EditCommand = new RelayCommand(_ => EditSupplier(), _ => SelectedSupplier != null);
        DeleteCommand = new RelayCommand(_ => DeleteSupplier(), _ => SelectedSupplier != null);

        LoadSuppliers();
    }

    public void LoadSuppliers()
    {
        using var db = new AppDbContext();

        var query = db.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(s => s.Name.Contains(SearchText));
        }

        Suppliers.Clear();
        foreach (var supplier in query.OrderBy(s => s.Id).ToList())
        {
            Suppliers.Add(supplier);
        }
    }

    private void AddSupplier()
    {
        var editVm = new SupplierEditViewModel(null);
        var window = new SupplierEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = editVm,
        };

        if (window.ShowDialog() == true)
        {
            using var db = new AppDbContext();
            var supplier = new Supplier();
            editVm.ApplyTo(supplier);
            db.Suppliers.Add(supplier);
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
            LoadSuppliers();
        }
    }

    private void EditSupplier()
    {
        var editVm = new SupplierEditViewModel(SelectedSupplier!);
        var window = new SupplierEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = editVm,
        };

        if (window.ShowDialog() == true)
        {
            using var db = new AppDbContext();
            var supplier = db.Suppliers.Find(SelectedSupplier!.Id);
            if (supplier != null)
            {
                editVm.ApplyTo(supplier);
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
            LoadSuppliers();
        }
    }

    private void DeleteSupplier()
    {
        var supplier = SelectedSupplier!;

        var answer = MessageBox.Show(
            $"确定删除供应商「{supplier.Name}」吗？",
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
            var toDelete = db.Suppliers.Find(supplier.Id);
            if (toDelete != null)
            {
                db.Suppliers.Remove(toDelete);
                db.SaveChanges();
            }
            LoadSuppliers();
        }
        catch (DbUpdateException)
        {
            // 供应商被采购单引用时，数据库 Restrict 约束拒绝删除
            MessageBox.Show(
                "该供应商已被采购单据引用，不能删除。",
                "无法删除",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
