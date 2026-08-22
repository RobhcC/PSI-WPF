namespace PSI.MVVM;

/// <summary>
/// 全局导航服务：任何 ViewModel 都能喊"我要去某某页"，
/// 而不需要认识 MainWindow——ViewModel 不认识任何界面类，才符合 MVVM 各层解耦的原则。
/// 单窗口应用用静态类足够：MainWindowViewModel 订阅 Navigated 事件，
/// 谁调 Navigate 就把内容区切到谁的 ViewModel。
/// </summary>
public static class NavigationService
{
    /// <summary>导航发生时触发，参数是目标页面的 ViewModel。</summary>
    public static event Action<ViewModelBase>? Navigated;

    /// <summary>发起导航：想跳哪页，就把那页的 ViewModel 传进来。</summary>
    public static void Navigate(ViewModelBase viewModel)
    {
        Navigated?.Invoke(viewModel);
    }
}
