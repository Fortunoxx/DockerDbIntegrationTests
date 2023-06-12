namespace SomeWebApiIntegrationTests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Dac;
using Testcontainers.MsSql;
using Xunit;

public class IntegrationTestFactory<TProgram, TDbContext1, TDbContext2> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext1 : DbContext where TDbContext2 : DbContext
{
    private const string PathToMigrations = "../../../../Database/Migrations";
    private const string PathToTestData = "../../../../Database/SeedData";
    private readonly MsSqlContainer _container = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private readonly string deployScriptPath = "../../../../Database/data/deployment_script.temp.sql";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(_container.GetConnectionString()); // get ip, port
        connectionStringBuilder.InitialCatalog = "TestDB";
        var connectionString1 = connectionStringBuilder.ConnectionString;
        connectionStringBuilder.InitialCatalog = "AnotherDB";
        var connectionString2 = connectionStringBuilder.ConnectionString;

        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<TDbContext1>();
            services.AddDbContext<TDbContext1>(options => options.UseSqlServer(connectionString1));
            services.EnsureDbCreated<TDbContext1>();

            services.RemoveDbContext<TDbContext2>();
            services.AddDbContext<TDbContext2>(options => options.UseSqlServer(connectionString2));
            services.EnsureDbCreated<TDbContext2>();

            services.AddMassTransitTestHarness();
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        FillDatabase(true);
    }

    public new async Task DisposeAsync() => await _container.DisposeAsync();

    private void FillDatabase(bool useDacPac)
    {
        var connectionString = _container.GetConnectionString();

        if (useDacPac)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "TestDB";
            // FillDbFromDacpac(builder);
            FillFromDacFx(builder);
            var evolveDacPac = new EvolveDb.Evolve(new SqlConnection(builder.ConnectionString), msg => Debug.WriteLine(msg))
            {
                Locations = new[] { PathToTestData, }
            };
            evolveDacPac.Migrate();
            return;
        }

        var evolve = new EvolveDb.Evolve(new SqlConnection(connectionString), msg => Debug.WriteLine(msg))
        {
            Locations = new[] { PathToMigrations, PathToTestData }
        };
        evolve.Migrate();
    }

    private void FillFromDacFx(SqlConnectionStringBuilder connectionStringBuilder)
    {
        using var dacpacStream = System.IO.File.Open("../../../../Database/StackOverflow2010.dacpac", System.IO.FileMode.Open);
        using DacPackage dacPackage = DacPackage.Load(dacpacStream);
       
        var dacService = new DacServices(connectionStringBuilder.ConnectionString);

        try
        {
            var dacDeployOptions = new DacDeployOptions { IgnorePermissions = true, };
            dacService.Deploy(dacPackage, connectionStringBuilder.InitialCatalog, true, dacDeployOptions);
        }
        catch(Exception ex)
        {
            throw ex;
        }
    }

    private void FillDbFromDacpac(SqlConnectionStringBuilder connectionStringBuilder, string databaseName = "master")
    {
        var arguments = new[] {
            "SqlPackage",
            "/Action:Publish",
            "/SourceFile:\"../../../../Database/StackOverflow2010.dacpac\"",
            $"/TargetUser:{connectionStringBuilder.UserID}",
            $"/TargetPassword:{connectionStringBuilder.Password}",
            $"/TargetServerName:{connectionStringBuilder.DataSource}",
            $"/TargetTrustServerCertificate:{connectionStringBuilder.TrustServerCertificate}",
            $"/TargetDatabaseName:{databaseName}",
        };

        var startInfo = new ProcessStartInfo()
        {
            FileName = "dotnet",
            Arguments = string.Join(" ", arguments),
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        process?.WaitForExit();
    }

    private async Task FillDbFromDacpac2()
    {
        using var dacpacStream = System.IO.File.Open("../../../../Database/StackOverflow2010.dacpac", System.IO.FileMode.Open);
        using DacPackage dacPackage = DacPackage.Load(dacpacStream);

        var dacService = new DacServices(_container.GetConnectionString());

        try
        {
            var deployScriptContent = dacService.GenerateDeployScript(dacPackage, "DACDB", new DacDeployOptions { IgnoreDefaultSchema = true, });
            System.IO.File.WriteAllText(deployScriptPath, deployScriptContent);
            await ReadSqlDeployScriptCopyAndExecInDockerContainerAsync(deployScriptContent);
        }
        catch
        {
            throw;
        }
    }

    private async Task ReadSqlDeployScriptCopyAndExecInDockerContainerAsync(string? deployScript = null)
    {
        if (deployScript == null)
        {
            deployScript = System.IO.File.ReadAllText(deployScriptPath);
        }

        var execResult = await this.CopyAndExecSqlDbCreateScriptContainerAsync(deployScript);

        const int successExitCode = 0;
        if (execResult.ExitCode != successExitCode)
        {
            throw new System.Exception(execResult.Stderr);
        }
    }

    public async Task<ExecResult> CopyAndExecSqlDbCreateScriptContainerAsync(string scriptContent, CancellationToken ct = default)
    {
        var constants = new
        {
            DefaultDataPathLinux = "/var/opt/mssql/data/",
            DefaultLogPathLinux = "/var/opt/mssql/log/",
            DefaultDataPathSqlEnvVar = "data",
            DefaultLogPathSqlEnvVar = "log",
            DatabaseName = "StackOverflow2010_TC",
            DefaultFilePrefix = "SOTC",
            DockerSqlDeployScriptPath = "/var/opt/mssql/script.sql",
            SqlServerDefaultConnection = "127.0.0.1,1433",
        };

        await this._container.CopyFileAsync(constants.DockerSqlDeployScriptPath, System.Text.Encoding.Default.GetBytes(scriptContent), 493, 0, 0, ct).ConfigureAwait(false);

        var sqlConnection = new SqlConnectionStringBuilder(this._container.GetConnectionString());

        string[] sqlCmds = new[]
        {
            "/opt/mssql-tools/bin/sqlcmd", "-b", "-r", "1",
            // "-S", sqlConnection.DataSource,
            "-S", constants.SqlServerDefaultConnection,
            "-U", sqlConnection.UserID, "-P", sqlConnection.Password,
            "-i", constants.DockerSqlDeployScriptPath,
            "-v", $"{constants.DefaultDataPathSqlEnvVar}={constants.DefaultDataPathLinux} {constants.DefaultLogPathSqlEnvVar}={constants.DefaultLogPathLinux}"
        };

        ExecResult execResult = await this._container.ExecAsync(sqlCmds, ct).ConfigureAwait(false);

        return execResult;
    }
}