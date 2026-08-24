using System.Windows;
using PSI.ViewModels;

namespace PSI.Windows;

/// <summary>采购单编辑窗口。保存逻辑全部在 PurchaseEditViewModel.Save()。</summary>
public partial class PurchaseEditWindow : Window
{
    public PurchaseEditWindow()
    {
        InitializeComponent();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is PurchaseEditViewModel viewModel && viewModel.Save())
        {
            DialogResult = true;
        }
    }
}
