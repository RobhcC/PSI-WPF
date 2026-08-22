using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PSI.MVVM;

/// <summary>
/// 所有需要"属性变了、通知界面刷新"的对象的基类。
/// WinForm 对照：WinForm 改完数据要手动 txtName.Text = "xxx" 刷界面；
/// WPF 界面通过绑定自动刷新，但前提是对象实现 INotifyPropertyChanged，
/// 在属性变化时"喊一嗓子"，绑定引擎听到就去重新读值、更新控件。
/// </summary>
public class ObservableObject : INotifyPropertyChanged
{
    /// <summary>
    /// 属性变化事件。界面绑定某属性时会自动订阅它；
    /// 事件一触发，WPF 就重新读属性值、刷新显示。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 触发属性变化通知。
    /// CallerMemberName 是编译期魔法：谁调用我，propertyName 自动就是谁的名字。
    /// 好处：不用手写字符串，属性改名时通知不会悄悄失效。
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 属性 setter 的标准三步：值没变不折腾 → 存新值 → 发通知。
    /// 以后所有 ViewModel 的属性都用它，一行顶三行。
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        // 用 EqualityComparer 而不是 == ：T 是泛型，可能是 int 也可能是对象，
        // Default 会自动选该类型自带的"相等"判断，对任何类型都成立
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false; // 新旧值一样，没必要让界面白刷一次
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
