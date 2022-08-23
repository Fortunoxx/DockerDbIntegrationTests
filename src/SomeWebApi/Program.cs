using MassTransit;
using Microsoft.EntityFrameworkCore;
using SomeWebApi.Consumers;
using SomeWebApi.Contracts;
using SomeWebApi.Courier.Activities;
using SomeWebApi.Courier.Helper;
using SomeWebApi.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SqlServerContext>(options =>
    options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=TestUserDatabase;User Id=<user>;Password=<password>;Trusted_Connection=True;")
);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    //// this works, but we don't need a mediator, we need a busmediator instead
    // x.AddMediator(cfg =>
    // {
    //     cfg.AddRequestClient<IStartProcessCommand>();
    //     cfg.AddConsumer<StartProcessCommandConsumer>();
    // });
    x.AddConsumer<StartProcessCommandConsumer>();
    x.AddRequestClient<IStartProcessCommand>();
    
    x.AddExecuteActivity<ProcessActivity, ProcessActivityArguments>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddScoped<ICourierService, CourierService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { } // this part