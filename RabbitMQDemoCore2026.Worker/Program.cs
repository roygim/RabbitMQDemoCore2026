using RabbitMQDemoCore2026.Worker;
using RabbitMQDemoCore2026.Worker.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

builder.Services.AddHostedService<ProductConsumerWork>();

var host = builder.Build();
host.Run();
