namespace PSI.ViewModels;

using PSI.MVVM;

/// <summary>
/// 主窗口的 ViewModel：当前页面状态 + 8 个菜单导航命令。
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel = new HomeViewModel();

    /// <summary>内容区当前显示的 ViewModel，绑定到主窗口的 ContentControl。</summary>
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    // 8 个菜单命令，一个菜单项一个命令
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
        ProductCommand = new RelayCommand(_ => NavigationService.Navigate(new ProductViewModel()));
        SupplierCommand = new RelayCommand(_ => NavigationService.Navigate(new SupplierViewModel()));
        CustomerCommand = new RelayCommand(_ => NavigationService.Navigate(new CustomerViewModel()));
        PurchaseCommand = new RelayCommand(_ => NavigationService.Navigate(new PurchaseViewModel()));
        SaleCommand = new RelayCommand(_ => NavigationService.Navigate(new SaleViewModel()));
        StockCommand = new RelayCommand(_ => NavigationService.Navigate(new StockViewModel()));
        ReportCommand = new RelayCommand(_ => NavigationService.Navigate(new ReportViewModel()));

        NavigationService.Navigated += vm => CurrentViewModel = vm;
    }
}
