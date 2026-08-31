using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PSI.MVVM;

/// <summary>
/// INotifyPropertyChanged 基类：属性变化时通知绑定界面刷新。
/// </summary>
public class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>触发属性变化通知。CallerMemberName 自动取调用方属性名，属性改名时不会失效。</summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>属性 setter 的标准写法：值没变不通知，变了才发。</summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
