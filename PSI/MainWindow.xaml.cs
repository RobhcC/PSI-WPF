using System.Windows;
using PSI.ViewModels;

namespace PSI;

/// <summary>
/// 主窗口，代码后置只负责挂 ViewModel。
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
    }
}
