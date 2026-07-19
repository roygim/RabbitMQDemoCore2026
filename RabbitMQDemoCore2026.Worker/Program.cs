using RabbitMQDemoCore2026.Infrastructure.Configuration;
using RabbitMQDemoCore2026.Worker.Consumers;
using RabbitMQDemoCore2026.Worker.Handlers;
using RabbitMQDemoCore2026.Worker.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

builder.Services.AddHostedService<ProductsDbConsumer>();

builder.Services.AddScoped<ProductCreatedHandler>();
builder.Services.AddScoped<ProductUpdatedHandler>();
builder.Services.AddScoped<ProductDeletedHandler>();

var host = builder.Build();
host.Run();
