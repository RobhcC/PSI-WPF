using System.Windows;
using PSI.ViewModels;

namespace PSI;

/// <summary>
/// 主窗口。MVVM 重构后：代码后置只剩"把 ViewModel 挂上来"一件事，
/// 原来的 8 个导航事件全部搬进了 MainWindowViewModel。
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // DataContext 是 WPF 绑定的"数据源"：
        // XAML 里所有 {Binding xxx} 都从 DataContext 上找属性
        DataContext = new MainWindowViewModel();
    }
}
