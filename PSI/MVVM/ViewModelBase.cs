namespace PSI.MVVM;

/// <summary>
/// 所有 ViewModel 的公共基类。
/// 目前只是继承 ObservableObject 的空壳，但它给"以后给全部 VM 统一加东西"
/// （比如加载中状态 IsBusy、页面标题）留了唯一挂点，成本为零、收益保留。
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}
