using RabbitMQDemoCore2026.Infrastructure.Configuration;
using RabbitMQDemoCore2026.Worker;
using RabbitMQDemoCore2026.Worker.Handlers;
using RabbitMQDemoCore2026.Worker.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

builder.Services.AddHostedService<ProductConsumerWork>();

builder.Services.AddScoped<ProductCreatedHandler>();

var host = builder.Build();
host.Run();
