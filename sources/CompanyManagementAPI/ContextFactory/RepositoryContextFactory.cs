using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Repository;

namespace CompanyManagementAPI.ContextFactory;

public class RepositoryContextFactory : IDesignTimeDbContextFactory<RepositoryContext>
{
    public RepositoryContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

        var builder = new DbContextOptionsBuilder<RepositoryContext>().UseNpgsql(configuration.GetConnectionString("sqlConnection"), b => b.MigrationsAssembly("CompanyManagementAPI"));

        //Return a new instance of our RepositoryContext class with provided options.
        return new RepositoryContext(builder.Options);
    }
}
