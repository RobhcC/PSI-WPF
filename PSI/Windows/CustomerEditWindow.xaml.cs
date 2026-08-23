using System.Windows;
using PSI.ViewModels;

namespace PSI.Windows;

/// <summary>客户编辑弹窗，结构与供应商编辑弹窗一致。</summary>
public partial class CustomerEditWindow : Window
{
    public CustomerEditWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomerEditViewModel viewModel && viewModel.Validate())
        {
            DialogResult = true;
        }
    }
}
