using RabbitMQ.Client;
using RabbitMQDemoCore2026.Api.Interfaces;
using RabbitMQDemoCore2026.Api.Services;
using RabbitMQDemoCore2026.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSingleton<IRabbitMqProducer, RabbitMqProducer>();
builder.Services.AddSingleton<IProductsProducer, ProductsProducer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.DocumentTitle = "Swagger UI - RabbitMQ Demo Core");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
