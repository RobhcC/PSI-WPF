using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PSI.Data;

/// <summary>
/// 设计时工厂：dotnet ef 迁移命令需要创建一个 DbContext，
/// 项目没用依赖注入容器，实现此接口让 EF 直接 new 一个。
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        return new AppDbContext();
    }
}
