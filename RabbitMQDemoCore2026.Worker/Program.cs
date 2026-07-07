using RabbitMQDemoCore2026.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ProductConsumerWork>();

var host = builder.Build();
host.Run();
