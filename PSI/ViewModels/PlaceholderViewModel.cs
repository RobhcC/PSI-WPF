namespace PSI.ViewModels;

using PSI.MVVM;

/// <summary>占位页的 ViewModel：只带一个标题属性，供界面绑定显示。</summary>
public class PlaceholderViewModel : ViewModelBase
{
    /// <summary>页面标题。只读：赋值一次后不变，不需要通知机制。</summary>
    public string Title { get; }

    public PlaceholderViewModel(string title)
    {
        Title = title + "（待实现）";
    }
}
