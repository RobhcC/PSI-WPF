using System.Windows;
using PSI.Pages;

namespace PSI;

/// <summary>
/// 主窗口：左侧菜单 + 右侧内容区。
/// 阶段1 故意用代码后置写导航（和 WinForm 一个套路），
/// 阶段2 引入 MVVM 基础设施后再重构，好亲身体会两种写法的差别。
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 程序启动后内容区先显示首页
        MainFrame.Navigate(new HomePage());
    }

    // 下面 8 个方法和 WinForm 的按钮事件一模一样：
    // XAML 里写 Click="MenuHome_Click"，等价于 WinForm 里 button.Click += MenuHome_Click
    private void MenuHome_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new HomePage());
    }

    private void MenuProduct_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PlaceholderPage("商品管理"));
    }

    private void MenuSupplier_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PlaceholderPage("供应商管理"));
    }

    private void MenuCustomer_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PlaceholderPage("客户管理"));
    }

    private void MenuPurchase_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PlaceholderPage("采购入库单"));
    }

    private void MenuSale_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PlaceholderPage("销售出库单"));
    }

    private void MenuStock_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PlaceholderPage("库存查询"));
    }

    private void MenuReport_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PlaceholderPage("月度统计"));
    }
}
