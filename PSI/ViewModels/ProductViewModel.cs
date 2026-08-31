using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;
using PSI.Windows;

namespace PSI.ViewModels;

/// <summary>
/// 商品列表页的 ViewModel：商品列表、选中行、搜索词和增删改查命令。
/// </summary>
public class ProductViewModel : ViewModelBase
{
    public ObservableCollection<Product> Products { get; } = new();

    private Product? _selectedProduct;

    /// <summary>表格当前选中行。</summary>
    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    private string _searchText = "";

    /// <summary>搜索框文字。</summary>
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public ProductViewModel()
    {
        SearchCommand = new RelayCommand(_ => LoadProducts());
        AddCommand = new RelayCommand(_ => AddProduct());
        // 未选中行时命令不可用，按钮自动变灰
        EditCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null);
        DeleteCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);

        LoadProducts();
    }

    /// <summary>从数据库加载商品列表（带名称过滤）。</summary>
    public void LoadProducts()
    {
        using var db = new AppDbContext();

        var query = db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(p => p.Name.Contains(SearchText));
        }

        Products.Clear();
        foreach (var product in query.OrderBy(p => p.Id).ToList())
        {
            Products.Add(product);
        }
    }

    private void AddProduct()
    {
        // VM 直接 new 弹窗而非对话框服务，单机小项目不值得多一层抽象
        var editVm = new ProductEditViewModel(null);
        var window = new ProductEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = editVm,
        };

        if (window.ShowDialog() == true)
        {
            using var db = new AppDbContext();
            var product = new Product();
            editVm.ApplyTo(product);
            db.Products.Add(product);
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // 数据库拒绝写入（如名称超长）：提示原因，放弃本次保存
                MessageBox.Show(
                    $"保存失败：{ex.InnerException?.Message ?? ex.Message}",
                    "保存失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            LoadProducts();
        }
    }

    private void EditProduct()
    {
        var editVm = new ProductEditViewModel(SelectedProduct!);
        var window = new ProductEditWindow
        {
            Owner = Application.Current.MainWindow,
            DataContext = editVm,
        };

        if (window.ShowDialog() == true)
        {
            // 重新按 Id 从库里查出要改的行（SelectedProduct 可能是旧快照），改完保存
            using var db = new AppDbContext();
            var product = db.Products.Find(SelectedProduct!.Id);
            if (product != null)
            {
                editVm.ApplyTo(product);
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
            LoadProducts();
        }
    }

    private void DeleteProduct()
    {
        var product = SelectedProduct!;

        var answer = MessageBox.Show(
            $"确定删除商品「{product.Name}」吗？",
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
            var toDelete = db.Products.Find(product.Id);
            if (toDelete != null)
            {
                db.Products.Remove(toDelete);
                db.SaveChanges();
            }
            LoadProducts();
        }
        catch (DbUpdateException)
        {
            // 商品被单据引用时，数据库的 Restrict 约束会拒绝删除并抛异常
            MessageBox.Show(
                "该商品已被单据引用，不能删除。",
                "无法删除",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
