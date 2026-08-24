using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 采购明细的"表格行"ViewModel：DataGrid 每一行绑一个它。
/// 为什么不直接绑 PurchaseOrderDetail 实体：① 实体是纯 POCO 不会发属性通知，
/// 数量改了界面金额不会刷新；② 明细要选商品（下拉），需要一个"待选状态"，
/// 实体里放可空导航属性会和 EF 的跟踪机制打架。行 VM 是纯界面层的草稿，
/// 点保存才翻译成实体——和编辑弹窗的草稿模式同一个思想。
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
                // 数量变了，金额（计算属性）也跟着变，手动通知它
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

    /// <summary>金额 = 数量 × 单价。只读计算属性，不给 setter。</summary>
    public decimal Amount => Quantity * UnitPrice;
}
