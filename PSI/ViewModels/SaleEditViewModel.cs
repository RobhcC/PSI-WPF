using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 销售出库单编辑的 ViewModel。与采购对称，多两件事：
/// ① 保存前校验库存余量（不足则拒绝保存）；
/// ② 库存是"扣减"而不是"增加"。
/// 同一商品出现多行时按商品汇总后统一校验，防止"每行都够、加起来不够"的漏判。
/// </summary>
public class SaleEditViewModel : ViewModelBase
{
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<SaleDetailRow> Details { get; } = new();

    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    private DateTime _orderDate = DateTime.Today;
    public DateTime OrderDate
    {
        get => _orderDate;
        set => SetProperty(ref _orderDate, value);
    }

    private SaleDetailRow? _selectedDetail;
    public SaleDetailRow? SelectedDetail
    {
        get => _selectedDetail;
        set => SetProperty(ref _selectedDetail, value);
    }

    public decimal TotalAmount
    {
        get
        {
            decimal total = 0;
            foreach (var row in Details)
            {
                total += row.Amount;
            }
            return total;
        }
    }

    public RelayCommand AddDetailCommand { get; }
    public RelayCommand RemoveDetailCommand { get; }

    public SaleEditViewModel()
    {
        using (var db = new AppDbContext())
        {
            foreach (var customer in db.Customers.OrderBy(c => c.Id).ToList())
            {
                Customers.Add(customer);
            }
            foreach (var product in db.Products.OrderBy(p => p.Id).ToList())
            {
                Products.Add(product);
            }
        }

        AddDetailCommand = new RelayCommand(_ => AddDetail());
        RemoveDetailCommand = new RelayCommand(_ => RemoveDetail(), _ => SelectedDetail != null);

        AddDetail();
    }

    private void AddDetail()
    {
        var row = new SaleDetailRow();
        row.PropertyChanged += OnRowPropertyChanged;
        Details.Add(row);
        SelectedDetail = row;
        OnPropertyChanged(nameof(TotalAmount));
    }

    private void RemoveDetail()
    {
        if (SelectedDetail == null)
        {
            return;
        }

        SelectedDetail.PropertyChanged -= OnRowPropertyChanged;
        Details.Remove(SelectedDetail);
        SelectedDetail = null;
        OnPropertyChanged(nameof(TotalAmount));
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SaleDetailRow.Amount))
        {
            OnPropertyChanged(nameof(TotalAmount));
        }
    }

    private bool Validate()
    {
        if (SelectedCustomer == null)
        {
            MessageBox.Show("请选择客户。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (Details.Count == 0)
        {
            MessageBox.Show("请至少添加一行明细。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        foreach (var row in Details)
        {
            if (row.SelectedProduct == null)
            {
                MessageBox.Show("存在未选择商品的明细行。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (row.Quantity <= 0)
            {
                MessageBox.Show($"商品「{row.SelectedProduct.Name}」数量必须大于 0。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (row.UnitPrice < 0)
            {
                MessageBox.Show($"商品「{row.SelectedProduct.Name}」单价不能为负。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        return true;
    }

    public bool Save()
    {
        if (!Validate())
        {
            return false;
        }

        using var db = new AppDbContext();

        // ---- 第一步：按商品汇总需求量（同一商品可能占多行）----
        var needed = new Dictionary<int, int>();
        foreach (var row in Details)
        {
            var productId = row.SelectedProduct!.Id;
            if (needed.ContainsKey(productId))
            {
                needed[productId] += row.Quantity;
            }
            else
            {
                needed[productId] = row.Quantity;
            }
        }

        // ---- 第二步：逐一校验库存余量，任何一个不足就整单拒绝 ----
        var stocks = db.Stocks
            .Where(s => needed.Keys.Contains(s.ProductId))
            .ToList();

        foreach (var entry in needed)
        {
            var stock = stocks.FirstOrDefault(s => s.ProductId == entry.Key);
            var available = stock?.Quantity ?? 0;
            if (available < entry.Value)
            {
                var product = db.Products.Find(entry.Key);
                MessageBox.Show(
                    $"商品「{product?.Name}」库存不足：现有 {available}，本单需要 {entry.Value}。",
                    "无法保存",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }
        }

        // ---- 第三步：校验全部通过，开始构建单据并扣减库存 ----
        var order = new SaleOrder
        {
            OrderNo = "XS" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            CustomerId = SelectedCustomer!.Id,
            OrderDate = OrderDate,
            TotalAmount = TotalAmount,
            CreatedAt = DateTime.Now,
        };

        foreach (var row in Details)
        {
            order.Details.Add(new SaleOrderDetail
            {
                ProductId = row.SelectedProduct!.Id,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice,
                Amount = row.Amount,
            });
        }

        foreach (var entry in needed)
        {
            stocks.First(s => s.ProductId == entry.Key).Quantity -= entry.Value;

            db.StockLogs.Add(new StockLog
            {
                ProductId = entry.Key,
                ChangeType = "销售出库",
                Quantity = entry.Value,
                OrderNo = order.OrderNo,
                CreatedAt = DateTime.Now,
            });
        }

        db.SaleOrders.Add(order);
        db.SaveChanges(); // 校验在前、扣减在后，全部变更一次原子提交

        return true;
    }
}
