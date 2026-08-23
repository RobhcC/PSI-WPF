# PSI 进销存管理系统

![.NET](https://img.shields.io/badge/.NET-8.0-blue) ![WPF](https://img.shields.io/badge/UI-WPF-purple) ![EF Core](https://img.shields.io/badge/ORM-EF%20Core%208-green) ![SQL Server](https://img.shields.io/badge/DB-SQL%20Server%20LocalDB-red)

一个使用 WPF + .NET 8 + EF Core 开发的桌面进销存管理系统（Purchase-Sales-Inventory），覆盖 **基础数据 → 采购入库 → 销售出库 → 库存联动 → 月度统计** 的完整业务闭环。

项目特点：**不依赖任何 MVVM 框架和第三方 UI 库**，ObservableObject / RelayCommand / ViewModelBase / NavigationService 全部手写实现，界面全部使用原生 WPF 控件。

## 技术选型

| 项 | 选择 | 理由 |
|---|---|---|
| 框架 | .NET 8 WPF（SDK 风格工程） | 当前主流桌面技术栈，dotnet CLI 原生支持 |
| 数据库 | SQL Server LocalDB | 本机零配置开发，后续可平滑切换完整 SQL Server 实例 |
| ORM | EF Core 8（Code First + Migrations） | 数据库结构随代码演进，迁移历史即数据库变更记录 |
| MVVM | 手写基础设施 | 每一行都在项目里，讲得清、改得动，不受框架升级牵制 |
| UI | 原生 WPF 控件 | 无第三方许可证问题，样式全部可控 |

## 功能模块

| 模块 | 说明 | 状态 |
|---|---|---|
| 首页 | 欢迎页 | ✅ |
| 商品管理 | 商品增删改查 | ✅ |
| 供应商管理 | 供应商增删改查 | ✅ |
| 客户管理 | 客户增删改查 | ✅ |
| 采购入库单 | 主从表录入，保存后库存增加 | 🚧 |
| 销售出库单 | 主从表录入，保存后库存扣减 | 🚧 |
| 库存查询 | 按商品查库存、库存变动流水 | 🚧 |
| 月度统计 | 采购/销售月度汇总 | 🚧 |

## 架构设计

采用经典 MVVM 分层（基础设施随开发阶段逐步引入）：

```
View (XAML 页面)          ← 只管界面长什么样
   ↕ 绑定 + 命令
ViewModel                 ← 界面状态与交互逻辑，不碰 UI 控件
   ↕ 调用
Model / EF Core 数据层     ← 实体、DbContext、数据库读写
```

## 快速开始

### 环境要求

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB（Visual Studio 安装时自带；或单独安装 [SQL Server Express](https://www.microsoft.com/zh-cn/sql-server/sql-server-downloads)）

### 运行

```bash
git clone https://github.com/RobhcC/PSI-WPF.git
cd PSI-WPF
dotnet run --project ./PSI
```

> 数据库接入（EF Core 迁移）完成后，此处将补充建库步骤。

## 项目结构

```
PSI-WPF/
├─ PSI.sln                 # 解决方案
└─ PSI/                    # 唯一工程（单项目结构）
   ├─ App.xaml             # 应用入口，指定启动窗口
   ├─ MainWindow.xaml      # 主窗口：左侧菜单 + 右侧内容区（MVVM 绑定驱动）
   ├─ MVVM/                # 手写 MVVM 基础设施
   │  ├─ ObservableObject  # INotifyPropertyChanged 封装，属性变化通知
   │  ├─ RelayCommand      # ICommand 封装，命令绑定
   │  ├─ ViewModelBase     # 所有 ViewModel 的公共基类
   │  └─ NavigationService # 全局导航服务
   ├─ Models/              # EF Core 实体（商品/供应商/客户/采购、销售主从单）
   ├─ Data/                # AppDbContext（Fluent API 配置 + 种子数据）与设计时工厂
   ├─ Migrations/          # EF Core 迁移（数据库结构版本历史）
   ├─ ViewModels/          # 各页面的 ViewModel（含商品列表与编辑弹窗）
   ├─ Windows/             # 弹窗（商品编辑等）
   └─ Pages/               # 功能页面（纯界面，无逻辑）
      ├─ HomePage          # 首页
      ├─ ProductPage       # 商品列表页
      └─ PlaceholderPage   # 占位页（待实现模块的临时入口）
```

## 开发轨迹

- [x] 工程初始化（解决方案 + WPF 工程 + gitignore）
- [x] 主窗口骨架与页面导航
- [x] MVVM 基础设施（ObservableObject / RelayCommand / ViewModelBase / NavigationService）
- [x] 数据层（实体、DbContext、EF 迁移、种子数据）
- [x] 商品模块 CRUD
- [x] 供应商 / 客户模块
- [ ] 采购入库单 / 销售出库单（库存联动）
- [ ] 库存查询 + 月度统计

提交遵循 [Conventional Commits](https://www.conventionalcommits.org/zh-hans/) 规范，每个功能独立成提交，完整开发历史见 [Commits](https://github.com/RobhcC/PSI-WPF/commits/main)。
