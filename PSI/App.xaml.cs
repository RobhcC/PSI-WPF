using System.Windows;
using System.Windows.Threading;

namespace PSI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 全局兜底：UI 线程上没被业务代码 catch 的异常，最后都落到这里。
    /// 异常处理分两层——保存/删除这类"数据库可能拒绝"的动作就地 catch，
    /// 给出具体原因（如"单号重复"）；其余异常（多为代码 bug、数据库连不上）
    /// 不在业务代码里包，让它一路冒泡到这里统一接住。
    /// 关键是 e.Handled = true：不设的话 WPF 默认行为是"程序已停止工作"整个退出，
    /// 单机软件不该让用户因为一个没料到的错误丢掉整个工作现场。
    /// </summary>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"发生未处理的错误：\n\n{e.Exception.Message}\n\n程序将继续运行。",
            "错误",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }
}
