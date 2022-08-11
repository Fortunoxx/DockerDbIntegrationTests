namespace SomeWebApi.Database;

using Microsoft.EntityFrameworkCore;
using SomeWebApi.Model;

public class SqlServerContext : DbContext
{
    public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options)
    { }

    public DbSet<User> Users { get; set; }
}
