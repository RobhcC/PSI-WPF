using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>销售明细的表格行 ViewModel，与采购明细行结构相同，默认带出的是销售价。</summary>
public class SaleDetailRow : ObservableObject
{
    private Product? _selectedProduct;

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value != null)
            {
                // 销售默认带售价（采购行带的是采购价——同一套结构，不同的默认值来源）
                UnitPrice = value.SalePrice;
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

    public decimal Amount => Quantity * UnitPrice;
}
