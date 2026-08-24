using Microsoft.EntityFrameworkCore;
using PSI.Models;

namespace PSI.Data;

/// <summary>
/// 数据库入口：EF Core 靠它知道"有哪些表、表怎么映射、外键怎么连"。
/// WinForm 对照：以前你用 SqlConnection + SqlAdapter 手写 SQL；
/// DbContext 把"写 SQL、开连接、读数据填对象"全包了，我们只操作 C# 对象。
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>连接字符串：LocalDB 实例 + 数据库名 PSI。
    /// 集中放一处，以后换数据库（如完整 SQL Server）只改这一行。</summary>
    public const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=PSI;Trusted_Connection=True;";

    // DbSet = 一张表。属性名 Products 会成为数据库里的表名（复数约定）
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
    public DbSet<SaleOrder> SaleOrders => Set<SaleOrder>();
    public DbSet<SaleOrderDetail> SaleOrderDetails => Set<SaleOrderDetail>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockLog> StockLogs => Set<StockLog>();

    /// <summary>告诉 EF 用哪个数据库。这里不用 appsettings.json 之类的配置文件：
    /// 单机桌面应用一个固定连接串，直接常量最直白。</summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(ConnectionString);
    }

    /// <summary>
    /// 模型配置（Fluent API）：约定之外的规则都写在这里——
    /// 字段长度、精度、外键关系、删除行为、种子数据。
    /// 写 C# 代码配置而不是贴 [Required] 特性在实体上，让实体保持纯数据类。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- 商品 ----------
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(50);
            e.Property(p => p.Category).HasMaxLength(20);
            e.Property(p => p.Unit).HasMaxLength(10);
            // decimal 若不指定精度，SQL Server 默认只保留 2 位小数，先声明清楚免得精度被悄悄砍
            e.Property(p => p.PurchasePrice).HasPrecision(18, 2);
            e.Property(p => p.SalePrice).HasPrecision(18, 2);
        });

        // ---------- 供应商 / 客户（结构相同，各自配置一遍，不为省几行做抽象） ----------
        modelBuilder.Entity<Supplier>(e =>
        {
            e.Property(s => s.Name).IsRequired().HasMaxLength(50);
            e.Property(s => s.ContactPerson).HasMaxLength(20);
            e.Property(s => s.Phone).HasMaxLength(20);
            e.Property(s => s.Address).HasMaxLength(100);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(50);
            e.Property(c => c.ContactPerson).HasMaxLength(20);
            e.Property(c => c.Phone).HasMaxLength(20);
            e.Property(c => c.Address).HasMaxLength(100);
        });

        // ---------- 采购单（主） ----------
        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.Property(o => o.OrderNo).IsRequired().HasMaxLength(20);
            e.HasIndex(o => o.OrderNo).IsUnique(); // 单据编号唯一，防止重复开单
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);

            // 供应商被单据引用后不许删，否则历史采购单会悬空 → Restrict
            e.HasOne(o => o.Supplier)
                .WithMany()
                .HasForeignKey(o => o.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // 外键：单据头删了，明细没有存在意义 → 级联删除
            e.HasMany(o => o.Details)
                .WithOne(d => d.PurchaseOrder)
                .HasForeignKey(d => d.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- 采购明细（从） ----------
        modelBuilder.Entity<PurchaseOrderDetail>(e =>
        {
            e.Property(d => d.UnitPrice).HasPrecision(18, 2);
            e.Property(d => d.Amount).HasPrecision(18, 2);

            // 外键：明细引用商品。商品被单据引用过就不许删 → Restrict（删除时数据库报错，
            // 界面层据此提示"该商品已被单据使用"，避免删了商品导致历史单据数据悬空）
            e.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- 销售单（主）/ 销售明细（从）：与采购完全对称 ----------
        modelBuilder.Entity<SaleOrder>(e =>
        {
            e.Property(o => o.OrderNo).IsRequired().HasMaxLength(20);
            e.HasIndex(o => o.OrderNo).IsUnique();
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);

            // 客户被单据引用后不许删，与供应商同理 → Restrict
            e.HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(o => o.Details)
                .WithOne(d => d.SaleOrder)
                .HasForeignKey(d => d.SaleOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SaleOrderDetail>(e =>
        {
            e.Property(d => d.UnitPrice).HasPrecision(18, 2);
            e.Property(d => d.Amount).HasPrecision(18, 2);

            e.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- 库存 ----------
        modelBuilder.Entity<Stock>(e =>
        {
            // 一个商品只允许一行库存：ProductId 唯一索引，防止出现两行互不知晓的库存记录
            e.HasIndex(s => s.ProductId).IsUnique();

            e.HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- 库存流水（快照式审计记录，只增不改） ----------
        modelBuilder.Entity<StockLog>(e =>
        {
            e.Property(l => l.ChangeType).IsRequired().HasMaxLength(20);
            e.Property(l => l.OrderNo).HasMaxLength(20);

            e.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- 种子数据 ----------
        // HasData 的数据会进迁移脚本：任何机器上执行 dotnet ef database update，
        // 库建好后自动带这批基础数据，开箱能演示。主键必须显式指定（固定值）。
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "PLC 模块 CPU1214C", Category = "控制器", Unit = "台", PurchasePrice = 1500, SalePrice = 1850 },
            new Product { Id = 2, Name = "触摸屏 TP700", Category = "人机界面", Unit = "台", PurchasePrice = 2200, SalePrice = 2680 },
            new Product { Id = 3, Name = "变频器 0.75kW", Category = "驱动", Unit = "台", PurchasePrice = 680, SalePrice = 890 },
            new Product { Id = 4, Name = "接近开关 PNP NO", Category = "传感器", Unit = "个", PurchasePrice = 35, SalePrice = 58 },
            new Product { Id = 5, Name = "光电传感器对射", Category = "传感器", Unit = "个", PurchasePrice = 62, SalePrice = 95 },
            new Product { Id = 6, Name = "中间继电器 24VDC", Category = "低压电器", Unit = "个", PurchasePrice = 18, SalePrice = 30 },
            new Product { Id = 7, Name = "屏蔽双绞线", Category = "线材", Unit = "米", PurchasePrice = 3.5m, SalePrice = 6 },
            new Product { Id = 8, Name = "开关电源 24V 5A", Category = "电源", Unit = "个", PurchasePrice = 85, SalePrice = 130 });

        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, Name = "华东工控设备有限公司", ContactPerson = "王工", Phone = "13800001111", Address = "上海市闵行区XX路88号" },
            new Supplier { Id = 2, Name = "深圳自动化元器件商行", ContactPerson = "李经理", Phone = "13900002222", Address = "深圳市宝安区XX路12号" },
            new Supplier { Id = 3, Name = "西门子授权分销商", ContactPerson = "张经理", Phone = "13700003333", Address = "广州市天河区XX路6号" });

        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "宏达机械制造厂", ContactPerson = "刘厂长", Phone = "13600004444", Address = "苏州市相城区XX路100号" },
            new Customer { Id = 2, Name = "蓝天包装设备公司", ContactPerson = "陈工", Phone = "13500005555", Address = "东莞市塘厦镇XX路3号" },
            new Customer { Id = 3, Name = "恒信水处理工程", ContactPerson = "赵工", Phone = "13400006666", Address = "武汉市汉阳区XX路21号" });
    }
}
