using System.Windows;
using PSI.ViewModels;

namespace PSI.Windows;

/// <summary>
/// 商品编辑弹窗。代码后置只做一件事：点确定时校验，通过才关窗并返回 true。
/// 校验逻辑在 ViewModel 里（Validate），窗口只是个壳——逻辑尽量不进界面类。
/// </summary>
public partial class ProductEditWindow : Window
{
    public ProductEditWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductEditViewModel viewModel && viewModel.Validate())
        {
            // DialogResult = true 是弹窗的"确认信号"，ShowDialog() 据此返回 true
            DialogResult = true;
        }
    }
}
