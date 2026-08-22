using System.Windows.Controls;

namespace PSI.Pages;

/// <summary>
/// 占位页：让菜单里所有模块先能点、能导航。
/// 真实功能页会在后续阶段逐个替换掉它，最终整个类删除。
/// MVVM 重构后标题由绑定提供，页面不再需要带参构造函数。
/// </summary>
public partial class PlaceholderPage : Page
{
    public PlaceholderPage()
    {
        InitializeComponent();
    }
}
