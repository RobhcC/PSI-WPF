using System.Windows;
using PSI.ViewModels;

namespace PSI.Windows;

/// <summary>供应商编辑弹窗，结构与商品编辑弹窗一致。</summary>
public partial class SupplierEditWindow : Window
{
    public SupplierEditWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SupplierEditViewModel viewModel && viewModel.Validate())
        {
            DialogResult = true;
        }
    }
}
