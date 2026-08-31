using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 采购明细的表格行 ViewModel：实体是 POCO 不发属性通知，界面联动由行 VM 负责，
/// 保存时才翻译成实体。
/// </summary>
public class PurchaseDetailRow : ObservableObject
{
    private Product? _selectedProduct;

    /// <summary>选中的商品。选完自动带出它的默认采购价（仍可手改）。</summary>
    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value != null)
            {
                UnitPrice = value.PurchasePrice;
            }
        }
    }

    private int _quantity = 1;

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                // 数量或单价变了，金额是计算属性，手动通知刷新
                OnPropertyChanged(nameof(Amount));
            }
        }
    }

    private decimal _unitPrice;

    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (SetProperty(ref _unitPrice, value))
            {
                OnPropertyChanged(nameof(Amount));
            }
        }
    }

    /// <summary>金额 = 数量 × 单价。</summary>
    public decimal Amount => Quantity * UnitPrice;
}
