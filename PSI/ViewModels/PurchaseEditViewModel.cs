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
/// 采购入库单编辑的 ViewModel：单据头（供应商/日期）+ 明细行集合 + 合计。
/// 保存 = 插入单据头和明细 + 库存增加 + 写库存流水，全部在同一次 SaveChanges 里完成。
/// EF Core 的 SaveChanges 内部自带事务：要么全部落库，要么全部回滚——
/// 不存在"单据存了但库存没加"的中间状态。
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

    /// <summary>合计金额 = 所有明细行金额之和（计算属性，由行变化通知刷新）。</summary>
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
        // 下拉框数据源：供应商和商品在开单前就固定了，一次性读出来足够
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

        // 开单先给一行空明细，用户直接选商品，少点一次按钮
        AddDetail();
    }

    private void AddDetail()
    {
        var row = new PurchaseDetailRow();
        // 盯住这一行：金额一变（数量或单价改了），合计就要刷新
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
    /// 保存：单据 + 明细 + 库存 + 流水，一次 SaveChanges 原子落库。
    /// 返回 true 表示保存成功（窗口可以关闭），false 表示校验或保存失败。
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
            // 单号 = 前缀 CG + 时间戳。单机单人使用，精度到秒足够避免重复；
            // 数据库另有唯一索引兜底，真撞号会抛异常而不是存进重复单
            OrderNo = "CG" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            SupplierId = SelectedSupplier!.Id,
            OrderDate = OrderDate,
            TotalAmount = TotalAmount,
            CreatedAt = DateTime.Now,
        };

        foreach (var row in Details)
        {
            // 只设外键 Id，不设导航属性：SelectedProduct 来自另一个已释放的
            // DbContext，塞给新 context 的导航属性会引发跟踪冲突
            order.Details.Add(new PurchaseOrderDetail
            {
                ProductId = row.SelectedProduct!.Id,
                Quantity = row.Quantity,
                UnitPrice = row.UnitPrice,
                Amount = row.Amount,
            });
        }

        // ---- 库存联动 + 流水：和单据在同一个 DbContext、同一次 SaveChanges ----
        // 先按商品汇总（同一商品可能占多行），再逐商品改库存、写流水，与销售单的 needed 字典同一思路。
        // 不汇总的漏洞：LINQ 查询只看数据库、看不见本事务里还没落库的新增，
        // 同一新商品占两行会各自 Add 一条 Stock，撞 ProductId 唯一索引。
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
            db.SaveChanges(); // 原子提交：单据/明细/库存/流水，四类变更要么全成、要么全无
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // 2601/2627 = 撞唯一索引。单号精确到秒，手工开单几乎撞不上，
            // 实际触发场景是双击保存按钮——第二次 Save 生成的还是同一秒的单号
            MessageBox.Show(
                "保存失败：单号重复，请稍候重试。",
                "保存失败",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false; // 窗口不关，用户可直接重试
        }
        catch (DbUpdateException ex)
        {
            // 其余数据库拒绝（意外情况）：给出原始原因，用户知道发生了什么再决定
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
