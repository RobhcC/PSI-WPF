using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;
using PSI.Windows;

namespace PSI.ViewModels;

/// <summary>
/// 商品列表页的 ViewModel：持商品列表、选中行、搜索词和四个操作命令。
/// 数据操作模式：每次操作开一个短命的 DbContext（using 用完即释放），
/// 桌面单机应用数据量小，这种"随用随开"最直白，也避免长连接带来的脏数据问题。
/// </summary>
public class ProductViewModel : ViewModelBase
{
    /// <summary>商品列表。用 ObservableCollection 而不是 List：
    /// 它在增删元素时会发通知，DataGrid 会自动跟着增删行；List 不会，界面会不刷新。</summary>
    public ObservableCollection<Product> Products { get; } = new();

    private Product? _selectedProduct;

    /// <summary>表格当前选中行。编辑/删除命令靠它工作，CanExecute 也盯它。</summary>
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
        // canExecute：没选中行时命令不可用，按钮自动变灰（CommandManager 定期重查）
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
        // 弹编辑窗（空表单）。这里 VM 直接 new 窗口，不是纯 MVVM（理论上该有对话框服务），
        // 但单机小项目引一层抽象不值得——面试被问到就照实说这个取舍。
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
            db.SaveChanges();
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
                db.SaveChanges();
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
            // 商品被采购/销售明细引用时，数据库的 Restrict 约束会拒绝删除并抛这个异常
            MessageBox.Show(
                "该商品已被单据引用，不能删除。",
                "无法删除",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
