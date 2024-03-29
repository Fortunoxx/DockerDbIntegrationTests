using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Refit;
using Serilog;
using SomeWebApi.Configuration;
using SomeWebApi.Consumers;
using SomeWebApi.Contracts.APIs;
using SomeWebApi.Courier.Activities;
using SomeWebApi.Courier.Helper;
using SomeWebApi.Database;

var builder = WebApplication.CreateBuilder(args);

// add serilog logging
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (File.Exists("secrets.json"))
{
    builder.Configuration.AddJsonFile("secrets.json");
}

var connectionStrings = builder.Configuration
    .GetSection(ConnectionStrings.ConfigSectionName)
    .Get<ConnectionStrings>();

builder.Services.AddDbContext<SqlServerContext>(options => options.UseSqlServer(connectionStrings?.SqlServerConnectionString));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<StartProcessCommandConsumer>();
    x.AddExecuteActivity<ProcessActivity, ProcessActivityArguments>();

    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    //! when using rabbitmq...
    // x.UsingRabbitMq((context, cfg) =>
    // {
    //     cfg.Host("localhost", "/", h =>
    //     {
    //         h.Username("guest");
    //         h.Password("guest");
    //     });
    // });
});

builder.Services.AddScoped<ICourierService, CourierService>();

//* refit & polly
var waitAndRetryConfig = builder.Configuration
    .GetSection(WaitAndRetryConfig.ConfigSectionName)
    .Get<WaitAndRetryConfig>();

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .Or<TimeoutRejectedException>() // Thrown by Polly's TimeoutPolicy if the inner call gets timeout.
    .WaitAndRetryAsync(waitAndRetryConfig!.Retry, _ => TimeSpan.FromMilliseconds(waitAndRetryConfig!.Wait), 
        onRetry: (outcome, timespan, retryAttempt) =>
        {
            Console.WriteLine("*** Delaying for {delay}ms, then making retry #{retryAttempt}");
            // Log.Information("Delaying for {delay}ms, then making retry #{retryAttempt}}", timespan.TotalMilliseconds, retryAttempt);
            // builder.Services.BuildServiceProvider().GetService<ILogger<IDocumentApi>>()?
            //     .LogWarning("Delaying for {delay}ms, then making retry {retry}.", timespan.TotalMilliseconds, retryAttempt);
        });

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(waitAndRetryConfig!.Timeout));

var externalSystemsConfig = builder.Configuration
    .GetSection(ExternalSystemsConfig.ConfigSectionName)
    .Get<ExternalSystemsConfig>();

builder.Services
    .AddRefitClient<IDocumentApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSystemsConfig!.DocumentServiceUri))
    // .AddHttpMessageHandler<AuthHeaderHandler>() // our endpoint doesn't need authentication
    .AddPolicyHandler(timeoutPolicy)
    .AddPolicyHandler(retryPolicy);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { } //* for integration tests
