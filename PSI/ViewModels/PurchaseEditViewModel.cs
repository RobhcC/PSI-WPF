using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PSI.Data;
using PSI.MVVM;
using PSI.Models;

namespace PSI.ViewModels;

/// <summary>
/// 采购入库单编辑的 ViewModel。保存时单据、明细、库存、流水
/// 在同一次 SaveChanges 里原子提交，不存在"单据存了库存没加"的中间状态。
/// </summary>
public class PurchaseEditViewModel : ViewModelBase
{
    public ObservableCollection<Supplier> Suppliers { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<PurchaseDetailRow> Details { get; } = new();

    private Supplier? _selectedSupplier;
    public Supplier? SelectedSupplier
    {
        get => _selectedSupplier;
        set => SetProperty(ref _selectedSupplier, value);
    }

    private DateTime _orderDate = DateTime.Today;
    public DateTime OrderDate
    {
        get => _orderDate;
        set => SetProperty(ref _orderDate, value);
    }

    private PurchaseDetailRow? _selectedDetail;
    public PurchaseDetailRow? SelectedDetail
    {
        get => _selectedDetail;
        set => SetProperty(ref _selectedDetail, value);
    }

    /// <summary>合计金额 = 所有明细行金额之和。</summary>
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

    public PurchaseEditViewModel()
    {
        // 下拉框数据源
        using (var db = new AppDbContext())
        {
            foreach (var supplier in db.Suppliers.OrderBy(s => s.Id).ToList())
            {
                Suppliers.Add(supplier);
            }
            foreach (var product in db.Products.OrderBy(p => p.Id).ToList())
            {
                Products.Add(product);
            }
        }

        AddDetailCommand = new RelayCommand(_ => AddDetail());
        RemoveDetailCommand = new RelayCommand(_ => RemoveDetail(), _ => SelectedDetail != null);

        // 开单先给一行空明细
        AddDetail();
    }

    private void AddDetail()
    {
        var row = new PurchaseDetailRow();
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
        if (e.PropertyName == nameof(PurchaseDetailRow.Amount))
        {
            OnPropertyChanged(nameof(TotalAmount));
        }
    }

    private bool Validate()
    {
        if (SelectedSupplier == null)
        {
            MessageBox.Show("请选择供应商。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
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

    /// <summary>
    /// 保存：单据 + 明细 + 库存 + 流水，一次原子提交。
    /// 返回 true 表示成功（窗口可关闭）。
    /// </summary>
    public bool Save()
    {
        if (!Validate())
        {
            return false;
        }

        using var db = new AppDbContext();

        var order = new PurchaseOrder
        {
            // 单号 = CG + 时间戳，数据库唯一索引兜底防重复
            OrderNo = "CG" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            SupplierId = SelectedSupplier!.Id,
            OrderDate = OrderDate,
            TotalAmount = TotalAmount,
            CreatedAt = DateTime.Now,
        };

        foreach (var row in Details)
        {
            // 只设外键 Id 不设导航属性：SelectedProduct 来自已释放的 DbContext，
            // 携带导航属性会引发跟踪冲突
            order.Details.Add(new PurchaseOrderDetail
            {
                ProductId = row.SelectedProduct!.Id,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice,
                Amount = row.Amount,
            });
        }

        // 库存联动 + 流水，与单据同一次 SaveChanges。同一商品先按行汇总，
        // 否则同一新商品占两行会各自 Add 一条 Stock，撞 ProductId 唯一索引
        foreach (var group in Details.GroupBy(row => row.SelectedProduct!.Id))
        {
            var productId = group.Key;
            var quantity = group.Sum(row => row.Quantity);

            var stock = db.Stocks.FirstOrDefault(s => s.ProductId == productId);
            if (stock == null)
            {
                // 该商品第一次入库：新建库存行
                db.Stocks.Add(new Stock { ProductId = productId, Quantity = quantity });
            }
            else
            {
                stock.Quantity += quantity;
            }

            db.StockLogs.Add(new StockLog
            {
                ProductId = productId,
                ChangeType = "采购入库",
                Quantity = quantity,
                OrderNo = order.OrderNo,
                CreatedAt = DateTime.Now,
            });
        }

        db.PurchaseOrders.Add(order);
        try
        {
            db.SaveChanges(); // 原子提交
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // 2601/2627 = 撞唯一索引，实际触发场景是双击保存按钮（同一秒内重复单号）
            MessageBox.Show(
                "保存失败：单号重复，请稍候重试。",
                "保存失败",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show(
                $"保存失败：{ex.InnerException?.Message ?? ex.Message}",
                "保存失败",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
