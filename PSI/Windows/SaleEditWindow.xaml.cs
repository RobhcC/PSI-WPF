using System.Windows;
using PSI.ViewModels;

namespace PSI.Windows;

/// <summary>销售单编辑窗口。保存逻辑在 SaleEditViewModel.Save()（含库存余量校验）。</summary>
public partial class SaleEditWindow : Window
{
    public SaleEditWindow()
    {
        InitializeComponent();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SaleEditViewModel viewModel && viewModel.Save())
        {
            DialogResult = true;
        }
    }
}
