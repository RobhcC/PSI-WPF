using System.Windows;
using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 商品编辑弹窗的 ViewModel。编辑的是草稿属性，确定时经 ApplyTo 写回实体，
/// 取消则不落库。
/// </summary>
public class ProductEditViewModel : ViewModelBase
{
    private readonly bool _isEdit;

    /// <summary>编辑模式传实体（把现有值抄进草稿）；新增模式传 null。</summary>
    public ProductEditViewModel(Product? product)
    {
        _isEdit = product != null;
        if (product != null)
        {
            _name = product.Name;
            _category = product.Category;
            _unit = product.Unit;
            _purchasePrice = product.PurchasePrice;
            _salePrice = product.SalePrice;
        }
    }

    /// <summary>窗口标题：新增/编辑二选一。</summary>
    public string Title => _isEdit ? "编辑商品" : "新增商品";

    private string _name = "";
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _category = "";
    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    private string _unit = "个";
    public string Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    private decimal _purchasePrice;
    public decimal PurchasePrice
    {
        get => _purchasePrice;
        set => SetProperty(ref _purchasePrice, value);
    }

    private decimal _salePrice;
    public decimal SalePrice
    {
        get => _salePrice;
        set => SetProperty(ref _salePrice, value);
    }

    /// <summary>点确定时校验草稿。返回 false 时弹窗不关闭。</summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("商品名称不能为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (PurchasePrice < 0 || SalePrice < 0)
        {
            MessageBox.Show("价格不能为负数。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    /// <summary>把草稿值写进实体（新增写进新实体，编辑写进查出来的实体）。</summary>
    public void ApplyTo(Product product)
    {
        product.Name = Name.Trim();
        product.Category = Category.Trim();
        product.Unit = Unit.Trim();
        product.PurchasePrice = PurchasePrice;
        product.SalePrice = SalePrice;
    }
}
