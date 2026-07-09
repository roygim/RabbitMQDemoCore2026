using RabbitMQDemoCore2026.Worker;
using RabbitMQDemoCore2026.Worker.RabbitMQ;
using RabbitMQDemoCore2026.Infrastructure.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

builder.Services.AddHostedService<ProductConsumerWork>();

var host = builder.Build();
host.Run();
