namespace SomeWebApiIntegrationTests;

using System.Diagnostics;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    private const string PathToMigrations = "../../../../Database/Migrations";
    private const string PathToTestData = "../../../../Database/SeedData";
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<TDbContext>();
            services.AddDbContext<TDbContext>(options => options.UseSqlServer(_container.GetConnectionString()));
            services.EnsureDbCreated<TDbContext>();
            services.AddMassTransitTestHarness();
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        FillDatabase();
    }

    public new async Task DisposeAsync() => await _container.DisposeAsync();

    private void FillDatabase()
    {
        var conn = new SqlConnection(_container.GetConnectionString());

        var evolve = new EvolveDb.Evolve(conn, msg => Debug.WriteLine(msg))
        {
            Locations = new[] { PathToMigrations, PathToTestData }
        };
        evolve.Migrate();
    }
}