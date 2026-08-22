using System.Windows.Controls;

namespace PSI.Pages;

/// <summary>
/// 占位页：让菜单里所有模块先能点、能导航。
/// 真实功能页会在后续阶段逐个替换掉它，最终整个类删除。
/// </summary>
public partial class PlaceholderPage : Page
{
    // 构造函数带一个标题参数：点哪个菜单，页面就显示哪个模块的名字
    public PlaceholderPage(string title)
    {
        InitializeComponent();
        TitleText.Text = title + "（待实现）";
    }
}
