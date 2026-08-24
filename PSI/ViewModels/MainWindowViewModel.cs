namespace PSI.ViewModels;

using PSI.MVVM;

/// <summary>
/// 主窗口的 ViewModel：持有"当前显示哪个页面"这个状态和左侧 8 个菜单命令。
/// MainWindow 的按钮全部绑定到这里的命令，代码后置不再有任何导航逻辑。
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel = new HomeViewModel();

    /// <summary>内容区当前显示的 ViewModel。属性一变，绑定它的 ContentControl 就换内容。</summary>
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    // 8 个菜单命令：一个菜单项对应一个命令，命令体里发起导航
    public RelayCommand HomeCommand { get; }
    public RelayCommand ProductCommand { get; }
    public RelayCommand SupplierCommand { get; }
    public RelayCommand CustomerCommand { get; }
    public RelayCommand PurchaseCommand { get; }
    public RelayCommand SaleCommand { get; }
    public RelayCommand StockCommand { get; }
    public RelayCommand ReportCommand { get; }

    public MainWindowViewModel()
    {
        HomeCommand = new RelayCommand(_ => NavigationService.Navigate(new HomeViewModel()));
        // 商品/供应商/客户模块均已实现，导航到真实页面
        ProductCommand = new RelayCommand(_ => NavigationService.Navigate(new ProductViewModel()));
        SupplierCommand = new RelayCommand(_ => NavigationService.Navigate(new SupplierViewModel()));
        CustomerCommand = new RelayCommand(_ => NavigationService.Navigate(new CustomerViewModel()));
        PurchaseCommand = new RelayCommand(_ => NavigationService.Navigate(new PurchaseViewModel()));
        SaleCommand = new RelayCommand(_ => NavigationService.Navigate(new PlaceholderViewModel("销售出库单")));
        StockCommand = new RelayCommand(_ => NavigationService.Navigate(new PlaceholderViewModel("库存查询")));
        ReportCommand = new RelayCommand(_ => NavigationService.Navigate(new PlaceholderViewModel("月度统计")));

        // 订阅全局导航：任何地方调 NavigationService.Navigate，主窗口内容区就跟着切换
        NavigationService.Navigated += vm => CurrentViewModel = vm;
    }
}
