namespace SomeWebApi.Database;

using Microsoft.EntityFrameworkCore;
using SomeWebApi.Model;

public class AnotherSqlServerContext : DbContext
{
    public AnotherSqlServerContext(DbContextOptions<AnotherSqlServerContext> options) : base(options)
    { }

    public DbSet<User> Users => Set<User>();
}
