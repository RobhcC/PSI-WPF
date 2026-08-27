using System.Windows.Controls;
using System.Windows.Input;
using PSI.ViewModels;

namespace PSI.Pages;

/// <summary>销售单列表页：纯界面，数据与命令来自 SaleViewModel。</summary>
public partial class SalePage : UserControl
{
    public SalePage()
    {
        InitializeComponent();
    }

    /// <summary>双击行查看明细：纯 UI 事件转调 VM 命令（与"查看明细"按钮同一入口）。</summary>
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SaleViewModel viewModel && viewModel.ViewCommand.CanExecute(null))
        {
            viewModel.ViewCommand.Execute(null);
        }
    }
}
