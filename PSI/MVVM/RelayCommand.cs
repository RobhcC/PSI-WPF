using System.Windows.Input;

namespace PSI.MVVM;

/// <summary>
/// 命令：把"用户点了按钮要干的事"包装成对象。
/// WinForm 对照：button.Click += OnSaveClick，事件直接指向一个方法；
/// MVVM 里按钮绑定 Command，命令由 ViewModel 提供——按钮不知道动作内容是什么，
/// 只负责"被点的时候执行它"。好处：同一个命令可以同时绑给按钮、菜单、快捷键，
/// 而且动作逻辑全部在 ViewModel 里，界面零逻辑。
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;         // 要执行的动作
    private readonly Func<object?, bool>? _canExecute; // 动作能不能执行（可选）

    /// <summary>
    /// 构造时把"干什么"和"能不能干"装进来。
    /// canExecute 传 null（或不传）表示任何时候都能执行。
    /// </summary>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    // 下面两个方法实现 ICommand 接口，由 WPF 调用：
    // WPF 用 CanExecute 决定按钮亮还是灰，用户点击时调 Execute

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    /// 通知 WPF"能不能执行"变了（比如按钮该从灰变亮）。
    /// 挂到 CommandManager.RequerySuggested 上：WPF 在界面空闲时会定期重查
    /// 所有命令的 CanExecute，所以"选中表格某行才允许删除"这类条件变化时，
    /// 按钮自动变灰/变亮，不需要我们手动去刷新。
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
