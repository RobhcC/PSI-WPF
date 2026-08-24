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
        // 库建好后自动带这批数据，开箱能演示。主键必须显式指定（固定值）。
        // 除了基础档案，还预置了采购/销售单、库存与流水——单据、库存、流水三者的
        // 数量关系严格按真实开单逻辑造（库存=采购合计-销售合计，流水每单每商品一行），
        // 保证库存查询和月度统计页一打开就是一套账实一致、能对上账的演示数据。
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "PLC 模块 CPU1214C", Category = "控制器", Unit = "台", PurchasePrice = 1500, SalePrice = 1850 },
            new Product { Id = 2, Name = "触摸屏 TP700", Category = "人机界面", Unit = "台", PurchasePrice = 2200, SalePrice = 2680 },
            new Product { Id = 3, Name = "变频器 0.75kW", Category = "驱动", Unit = "台", PurchasePrice = 680, SalePrice = 890 },
            new Product { Id = 4, Name = "接近开关 PNP NO", Category = "传感器", Unit = "个", PurchasePrice = 35, SalePrice = 58 },
            new Product { Id = 5, Name = "光电传感器对射", Category = "传感器", Unit = "个", PurchasePrice = 62, SalePrice = 95 },
            new Product { Id = 6, Name = "中间继电器 24VDC", Category = "低压电器", Unit = "个", PurchasePrice = 18, SalePrice = 30 },
            new Product { Id = 7, Name = "屏蔽双绞线", Category = "线材", Unit = "米", PurchasePrice = 3.5m, SalePrice = 6 },
            new Product { Id = 8, Name = "开关电源 24V 5A", Category = "电源", Unit = "个", PurchasePrice = 85, SalePrice = 130 },
            new Product { Id = 9, Name = "伺服电机 750W", Category = "驱动", Unit = "台", PurchasePrice = 1350, SalePrice = 1750 },
            new Product { Id = 10, Name = "编码器线缆 2米", Category = "线材", Unit = "根", PurchasePrice = 45, SalePrice = 75 },
            new Product { Id = 11, Name = "空气开关 2P 32A", Category = "低压电器", Unit = "个", PurchasePrice = 28, SalePrice = 48 },
            new Product { Id = 12, Name = "光电开关漫反射", Category = "传感器", Unit = "个", PurchasePrice = 55, SalePrice = 88 },
            new Product { Id = 13, Name = "组态软件授权", Category = "软件", Unit = "套", PurchasePrice = 800, SalePrice = 1200 },
            new Product { Id = 14, Name = "工业交换机 8口", Category = "通讯", Unit = "台", PurchasePrice = 160, SalePrice = 260 },
            new Product { Id = 15, Name = "信号隔离器", Category = "低压电器", Unit = "个", PurchasePrice = 65, SalePrice = 105 },
            new Product { Id = 16, Name = "急停按钮盒", Category = "低压电器", Unit = "个", PurchasePrice = 22, SalePrice = 40 });

        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, Name = "华东工控设备有限公司", ContactPerson = "王工", Phone = "13800001111", Address = "上海市闵行区XX路88号" },
            new Supplier { Id = 2, Name = "深圳自动化元器件商行", ContactPerson = "李经理", Phone = "13900002222", Address = "深圳市宝安区XX路12号" },
            new Supplier { Id = 3, Name = "西门子授权分销商", ContactPerson = "张经理", Phone = "13700003333", Address = "广州市天河区XX路6号" },
            new Supplier { Id = 4, Name = "北京中科自控设备", ContactPerson = "孙工", Phone = "13311117777", Address = "北京市海淀区XX路15号" },
            new Supplier { Id = 5, Name = "武汉光谷传感器科技", ContactPerson = "周工", Phone = "13222228888", Address = "武汉市东湖高新区XX路9号" },
            new Supplier { Id = 6, Name = "东莞长安机电市场", ContactPerson = "吴老板", Phone = "13133339999", Address = "东莞市长安镇XX路56号" });

        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "宏达机械制造厂", ContactPerson = "刘厂长", Phone = "13600004444", Address = "苏州市相城区XX路100号" },
            new Customer { Id = 2, Name = "蓝天包装设备公司", ContactPerson = "陈工", Phone = "13500005555", Address = "东莞市塘厦镇XX路3号" },
            new Customer { Id = 3, Name = "恒信水处理工程", ContactPerson = "赵工", Phone = "13400006666", Address = "武汉市汉阳区XX路21号" },
            new Customer { Id = 4, Name = "广州明辉食品机械", ContactPerson = "黄工", Phone = "13044441111", Address = "广州市番禺区XX路18号" },
            new Customer { Id = 5, Name = "青岛海纳重工装备", ContactPerson = "郑工", Phone = "12855552222", Address = "青岛市黄岛区XX路77号" },
            new Customer { Id = 6, Name = "成都锦程自动化集成", ContactPerson = "何工", Phone = "12766663333", Address = "成都市高新区XX路32号" });

        // ---------- 演示单据：采购 5 张、销售 5 张，日期摊在 2026 年 4~8 月，月度统计页有数据可看。
        // 单号沿用程序生成的 CG/XS + 时间戳格式；金额=明细行之和，与保存时的计算口径一致 ----------
        modelBuilder.Entity<PurchaseOrder>().HasData(
            new PurchaseOrder { Id = 1, OrderNo = "CG20260408093000", SupplierId = 3, OrderDate = new DateTime(2026, 4, 8, 9, 30, 0), TotalAmount = 36360, CreatedAt = new DateTime(2026, 4, 8, 9, 30, 0) },
            new PurchaseOrder { Id = 2, OrderNo = "CG20260512090000", SupplierId = 1, OrderDate = new DateTime(2026, 5, 12, 9, 0, 0), TotalAmount = 16650, CreatedAt = new DateTime(2026, 5, 12, 9, 0, 0) },
            new PurchaseOrder { Id = 3, OrderNo = "CG20260615093000", SupplierId = 5, OrderDate = new DateTime(2026, 6, 15, 9, 30, 0), TotalAmount = 17660, CreatedAt = new DateTime(2026, 6, 15, 9, 30, 0) },
            new PurchaseOrder { Id = 4, OrderNo = "CG20260718090000", SupplierId = 4, OrderDate = new DateTime(2026, 7, 18, 9, 0, 0), TotalAmount = 22200, CreatedAt = new DateTime(2026, 7, 18, 9, 0, 0) },
            new PurchaseOrder { Id = 5, OrderNo = "CG20260810093000", SupplierId = 2, OrderDate = new DateTime(2026, 8, 10, 9, 30, 0), TotalAmount = 34740, CreatedAt = new DateTime(2026, 8, 10, 9, 30, 0) });

        modelBuilder.Entity<PurchaseOrderDetail>().HasData(
            // 单据 1 合计 36360
            new PurchaseOrderDetail { Id = 1, PurchaseOrderId = 1, ProductId = 1, Quantity = 10, UnitPrice = 1500, Amount = 15000 },
            new PurchaseOrderDetail { Id = 2, PurchaseOrderId = 1, ProductId = 2, Quantity = 6, UnitPrice = 2200, Amount = 13200 },
            new PurchaseOrderDetail { Id = 3, PurchaseOrderId = 1, ProductId = 3, Quantity = 12, UnitPrice = 680, Amount = 8160 },
            // 单据 2 合计 16650
            new PurchaseOrderDetail { Id = 4, PurchaseOrderId = 2, ProductId = 4, Quantity = 200, UnitPrice = 35, Amount = 7000 },
            new PurchaseOrderDetail { Id = 5, PurchaseOrderId = 2, ProductId = 6, Quantity = 300, UnitPrice = 18, Amount = 5400 },
            new PurchaseOrderDetail { Id = 6, PurchaseOrderId = 2, ProductId = 8, Quantity = 50, UnitPrice = 85, Amount = 4250 },
            // 单据 3 合计 17660
            new PurchaseOrderDetail { Id = 7, PurchaseOrderId = 3, ProductId = 5, Quantity = 150, UnitPrice = 62, Amount = 9300 },
            new PurchaseOrderDetail { Id = 8, PurchaseOrderId = 3, ProductId = 12, Quantity = 120, UnitPrice = 55, Amount = 6600 },
            new PurchaseOrderDetail { Id = 9, PurchaseOrderId = 3, ProductId = 16, Quantity = 80, UnitPrice = 22, Amount = 1760 },
            // 单据 4 合计 22200
            new PurchaseOrderDetail { Id = 10, PurchaseOrderId = 4, ProductId = 1, Quantity = 5, UnitPrice = 1500, Amount = 7500 },
            new PurchaseOrderDetail { Id = 11, PurchaseOrderId = 4, ProductId = 9, Quantity = 8, UnitPrice = 1350, Amount = 10800 },
            new PurchaseOrderDetail { Id = 12, PurchaseOrderId = 4, ProductId = 15, Quantity = 60, UnitPrice = 65, Amount = 3900 },
            // 单据 5 合计 34740
            new PurchaseOrderDetail { Id = 13, PurchaseOrderId = 5, ProductId = 7, Quantity = 1000, UnitPrice = 3.5m, Amount = 3500 },
            new PurchaseOrderDetail { Id = 14, PurchaseOrderId = 5, ProductId = 10, Quantity = 200, UnitPrice = 45, Amount = 9000 },
            new PurchaseOrderDetail { Id = 15, PurchaseOrderId = 5, ProductId = 11, Quantity = 80, UnitPrice = 28, Amount = 2240 },
            new PurchaseOrderDetail { Id = 16, PurchaseOrderId = 5, ProductId = 13, Quantity = 20, UnitPrice = 800, Amount = 16000 },
            new PurchaseOrderDetail { Id = 17, PurchaseOrderId = 5, ProductId = 14, Quantity = 25, UnitPrice = 160, Amount = 4000 });

        modelBuilder.Entity<SaleOrder>().HasData(
            new SaleOrder { Id = 1, OrderNo = "XS20260520093000", CustomerId = 1, OrderDate = new DateTime(2026, 5, 20, 9, 30, 0), TotalAmount = 14850, CreatedAt = new DateTime(2026, 5, 20, 9, 30, 0) },
            new SaleOrder { Id = 2, OrderNo = "XS20260625090000", CustomerId = 2, OrderDate = new DateTime(2026, 6, 25, 9, 0, 0), TotalAmount = 15280, CreatedAt = new DateTime(2026, 6, 25, 9, 0, 0) },
            new SaleOrder { Id = 3, OrderNo = "XS20260728090000", CustomerId = 4, OrderDate = new DateTime(2026, 7, 28, 9, 0, 0), TotalAmount = 11700, CreatedAt = new DateTime(2026, 7, 28, 9, 0, 0) },
            new SaleOrder { Id = 4, OrderNo = "XS20260815090000", CustomerId = 5, OrderDate = new DateTime(2026, 8, 15, 9, 0, 0), TotalAmount = 15850, CreatedAt = new DateTime(2026, 8, 15, 9, 0, 0) },
            new SaleOrder { Id = 5, OrderNo = "XS20260822093000", CustomerId = 6, OrderDate = new DateTime(2026, 8, 22, 9, 30, 0), TotalAmount = 14190, CreatedAt = new DateTime(2026, 8, 22, 9, 30, 0) });

        modelBuilder.Entity<SaleOrderDetail>().HasData(
            // 单据 1 合计 14850
            new SaleOrderDetail { Id = 1, SaleOrderId = 1, ProductId = 1, Quantity = 4, UnitPrice = 1850, Amount = 7400 },
            new SaleOrderDetail { Id = 2, SaleOrderId = 1, ProductId = 3, Quantity = 5, UnitPrice = 890, Amount = 4450 },
            new SaleOrderDetail { Id = 3, SaleOrderId = 1, ProductId = 6, Quantity = 100, UnitPrice = 30, Amount = 3000 },
            // 单据 2 合计 15280
            new SaleOrderDetail { Id = 4, SaleOrderId = 2, ProductId = 2, Quantity = 3, UnitPrice = 2680, Amount = 8040 },
            new SaleOrderDetail { Id = 5, SaleOrderId = 2, ProductId = 4, Quantity = 80, UnitPrice = 58, Amount = 4640 },
            new SaleOrderDetail { Id = 6, SaleOrderId = 2, ProductId = 8, Quantity = 20, UnitPrice = 130, Amount = 2600 },
            // 单据 3 合计 11700
            new SaleOrderDetail { Id = 7, SaleOrderId = 3, ProductId = 5, Quantity = 60, UnitPrice = 95, Amount = 5700 },
            new SaleOrderDetail { Id = 8, SaleOrderId = 3, ProductId = 12, Quantity = 50, UnitPrice = 88, Amount = 4400 },
            new SaleOrderDetail { Id = 9, SaleOrderId = 3, ProductId = 16, Quantity = 40, UnitPrice = 40, Amount = 1600 },
            // 单据 4 合计 15850
            new SaleOrderDetail { Id = 10, SaleOrderId = 4, ProductId = 9, Quantity = 5, UnitPrice = 1750, Amount = 8750 },
            new SaleOrderDetail { Id = 11, SaleOrderId = 4, ProductId = 14, Quantity = 10, UnitPrice = 260, Amount = 2600 },
            new SaleOrderDetail { Id = 12, SaleOrderId = 4, ProductId = 10, Quantity = 60, UnitPrice = 75, Amount = 4500 },
            // 单据 5 合计 14190
            new SaleOrderDetail { Id = 13, SaleOrderId = 5, ProductId = 13, Quantity = 8, UnitPrice = 1200, Amount = 9600 },
            new SaleOrderDetail { Id = 14, SaleOrderId = 5, ProductId = 11, Quantity = 30, UnitPrice = 48, Amount = 1440 },
            new SaleOrderDetail { Id = 15, SaleOrderId = 5, ProductId = 15, Quantity = 30, UnitPrice = 105, Amount = 3150 });

        // ---------- 库存结存：每个商品的采购合计 - 销售合计，与真实开单维护出的结果一致 ----------
        modelBuilder.Entity<Stock>().HasData(
            new Stock { Id = 1, ProductId = 1, Quantity = 11 },
            new Stock { Id = 2, ProductId = 2, Quantity = 3 },
            new Stock { Id = 3, ProductId = 3, Quantity = 7 },
            new Stock { Id = 4, ProductId = 4, Quantity = 120 },
            new Stock { Id = 5, ProductId = 5, Quantity = 90 },
            new Stock { Id = 6, ProductId = 6, Quantity = 200 },
            new Stock { Id = 7, ProductId = 7, Quantity = 1000 },
            new Stock { Id = 8, ProductId = 8, Quantity = 30 },
            new Stock { Id = 9, ProductId = 9, Quantity = 3 },
            new Stock { Id = 10, ProductId = 10, Quantity = 140 },
            new Stock { Id = 11, ProductId = 11, Quantity = 50 },
            new Stock { Id = 12, ProductId = 12, Quantity = 70 },
            new Stock { Id = 13, ProductId = 13, Quantity = 12 },
            new Stock { Id = 14, ProductId = 14, Quantity = 15 },
            new Stock { Id = 15, ProductId = 15, Quantity = 30 },
            new Stock { Id = 16, ProductId = 16, Quantity = 40 });

        // ---------- 库存流水：与保存逻辑一致，每张单的每个商品一行，只增不改 ----------
        modelBuilder.Entity<StockLog>().HasData(
            // 采购入库 17 行（对应上面 5 张采购单）
            new StockLog { Id = 1, ProductId = 1, ChangeType = "采购入库", Quantity = 10, OrderNo = "CG20260408093000", CreatedAt = new DateTime(2026, 4, 8, 9, 30, 0) },
            new StockLog { Id = 2, ProductId = 2, ChangeType = "采购入库", Quantity = 6, OrderNo = "CG20260408093000", CreatedAt = new DateTime(2026, 4, 8, 9, 30, 0) },
            new StockLog { Id = 3, ProductId = 3, ChangeType = "采购入库", Quantity = 12, OrderNo = "CG20260408093000", CreatedAt = new DateTime(2026, 4, 8, 9, 30, 0) },
            new StockLog { Id = 4, ProductId = 4, ChangeType = "采购入库", Quantity = 200, OrderNo = "CG20260512090000", CreatedAt = new DateTime(2026, 5, 12, 9, 0, 0) },
            new StockLog { Id = 5, ProductId = 6, ChangeType = "采购入库", Quantity = 300, OrderNo = "CG20260512090000", CreatedAt = new DateTime(2026, 5, 12, 9, 0, 0) },
            new StockLog { Id = 6, ProductId = 8, ChangeType = "采购入库", Quantity = 50, OrderNo = "CG20260512090000", CreatedAt = new DateTime(2026, 5, 12, 9, 0, 0) },
            new StockLog { Id = 7, ProductId = 5, ChangeType = "采购入库", Quantity = 150, OrderNo = "CG20260615093000", CreatedAt = new DateTime(2026, 6, 15, 9, 30, 0) },
            new StockLog { Id = 8, ProductId = 12, ChangeType = "采购入库", Quantity = 120, OrderNo = "CG20260615093000", CreatedAt = new DateTime(2026, 6, 15, 9, 30, 0) },
            new StockLog { Id = 9, ProductId = 16, ChangeType = "采购入库", Quantity = 80, OrderNo = "CG20260615093000", CreatedAt = new DateTime(2026, 6, 15, 9, 30, 0) },
            new StockLog { Id = 10, ProductId = 1, ChangeType = "采购入库", Quantity = 5, OrderNo = "CG20260718090000", CreatedAt = new DateTime(2026, 7, 18, 9, 0, 0) },
            new StockLog { Id = 11, ProductId = 9, ChangeType = "采购入库", Quantity = 8, OrderNo = "CG20260718090000", CreatedAt = new DateTime(2026, 7, 18, 9, 0, 0) },
            new StockLog { Id = 12, ProductId = 15, ChangeType = "采购入库", Quantity = 60, OrderNo = "CG20260718090000", CreatedAt = new DateTime(2026, 7, 18, 9, 0, 0) },
            new StockLog { Id = 13, ProductId = 7, ChangeType = "采购入库", Quantity = 1000, OrderNo = "CG20260810093000", CreatedAt = new DateTime(2026, 8, 10, 9, 30, 0) },
            new StockLog { Id = 14, ProductId = 10, ChangeType = "采购入库", Quantity = 200, OrderNo = "CG20260810093000", CreatedAt = new DateTime(2026, 8, 10, 9, 30, 0) },
            new StockLog { Id = 15, ProductId = 11, ChangeType = "采购入库", Quantity = 80, OrderNo = "CG20260810093000", CreatedAt = new DateTime(2026, 8, 10, 9, 30, 0) },
            new StockLog { Id = 16, ProductId = 13, ChangeType = "采购入库", Quantity = 20, OrderNo = "CG20260810093000", CreatedAt = new DateTime(2026, 8, 10, 9, 30, 0) },
            new StockLog { Id = 17, ProductId = 14, ChangeType = "采购入库", Quantity = 25, OrderNo = "CG20260810093000", CreatedAt = new DateTime(2026, 8, 10, 9, 30, 0) },
            // 销售出库 15 行（对应上面 5 张销售单）
            new StockLog { Id = 18, ProductId = 1, ChangeType = "销售出库", Quantity = 4, OrderNo = "XS20260520093000", CreatedAt = new DateTime(2026, 5, 20, 9, 30, 0) },
            new StockLog { Id = 19, ProductId = 3, ChangeType = "销售出库", Quantity = 5, OrderNo = "XS20260520093000", CreatedAt = new DateTime(2026, 5, 20, 9, 30, 0) },
            new StockLog { Id = 20, ProductId = 6, ChangeType = "销售出库", Quantity = 100, OrderNo = "XS20260520093000", CreatedAt = new DateTime(2026, 5, 20, 9, 30, 0) },
            new StockLog { Id = 21, ProductId = 2, ChangeType = "销售出库", Quantity = 3, OrderNo = "XS20260625090000", CreatedAt = new DateTime(2026, 6, 25, 9, 0, 0) },
            new StockLog { Id = 22, ProductId = 4, ChangeType = "销售出库", Quantity = 80, OrderNo = "XS20260625090000", CreatedAt = new DateTime(2026, 6, 25, 9, 0, 0) },
            new StockLog { Id = 23, ProductId = 8, ChangeType = "销售出库", Quantity = 20, OrderNo = "XS20260625090000", CreatedAt = new DateTime(2026, 6, 25, 9, 0, 0) },
            new StockLog { Id = 24, ProductId = 5, ChangeType = "销售出库", Quantity = 60, OrderNo = "XS20260728090000", CreatedAt = new DateTime(2026, 7, 28, 9, 0, 0) },
            new StockLog { Id = 25, ProductId = 12, ChangeType = "销售出库", Quantity = 50, OrderNo = "XS20260728090000", CreatedAt = new DateTime(2026, 7, 28, 9, 0, 0) },
            new StockLog { Id = 26, ProductId = 16, ChangeType = "销售出库", Quantity = 40, OrderNo = "XS20260728090000", CreatedAt = new DateTime(2026, 7, 28, 9, 0, 0) },
            new StockLog { Id = 27, ProductId = 9, ChangeType = "销售出库", Quantity = 5, OrderNo = "XS20260815090000", CreatedAt = new DateTime(2026, 8, 15, 9, 0, 0) },
            new StockLog { Id = 28, ProductId = 14, ChangeType = "销售出库", Quantity = 10, OrderNo = "XS20260815090000", CreatedAt = new DateTime(2026, 8, 15, 9, 0, 0) },
            new StockLog { Id = 29, ProductId = 10, ChangeType = "销售出库", Quantity = 60, OrderNo = "XS20260815090000", CreatedAt = new DateTime(2026, 8, 15, 9, 0, 0) },
            new StockLog { Id = 30, ProductId = 13, ChangeType = "销售出库", Quantity = 8, OrderNo = "XS20260822093000", CreatedAt = new DateTime(2026, 8, 22, 9, 30, 0) },
            new StockLog { Id = 31, ProductId = 11, ChangeType = "销售出库", Quantity = 30, OrderNo = "XS20260822093000", CreatedAt = new DateTime(2026, 8, 22, 9, 30, 0) },
            new StockLog { Id = 32, ProductId = 15, ChangeType = "销售出库", Quantity = 30, OrderNo = "XS20260822093000", CreatedAt = new DateTime(2026, 8, 22, 9, 30, 0) });
    }
}
