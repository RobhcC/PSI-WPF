namespace PSI.MVVM;

/// <summary>
/// 全局导航服务：ViewModel 发起跳转但不依赖主窗口，保持层间解耦。
/// 单窗口应用用静态类足够，MainWindowViewModel 订阅 Navigated 事件切换内容区。
/// </summary>
public static class NavigationService
{
    /// <summary>导航发生时触发，参数为目标页面的 ViewModel。</summary>
    public static event Action<ViewModelBase>? Navigated;

    public static void Navigate(ViewModelBase viewModel)
    {
        Navigated?.Invoke(viewModel);
    }
}
