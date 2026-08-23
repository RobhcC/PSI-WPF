using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PSI.Data;

/// <summary>
/// 设计时工厂：dotnet ef 执行迁移命令时，需要"凭空"创建一个 DbContext 来分析模型。
/// 项目没有用依赖注入容器，所以实现这个接口告诉 EF Core：直接 new 就行。
/// 平时写业务代码用不到它，只有迁移命令会调用。
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        return new AppDbContext();
    }
}
