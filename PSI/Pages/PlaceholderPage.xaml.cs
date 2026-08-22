using System.Windows.Controls;

namespace PSI.Pages;

/// <summary>
/// 占位页：让菜单里所有模块先能点、能导航。
/// 真实功能页会在后续阶段逐个替换掉它，最终整个类删除。
/// UserControl（而非 Page）才能宿主在 ContentControl 中。
/// </summary>
public partial class PlaceholderPage : UserControl
{
    public PlaceholderPage()
    {
        InitializeComponent();
    }
}
