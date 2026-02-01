using BlueExplorer.ServiceBus.Api.Endpoints;
using ServiceBus.Application.Configuration;
using ServiceBus.Application.Interfaces;
using ServiceBus.Application.Services;
using ServiceBus.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection("ServiceBus"));
builder.Services.AddSingleton<IConnectionStore, JsonConnectionStore>();
builder.Services.AddSingleton<IServiceBusConnectionProvider, ApiServiceBusConnectionProvider>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<ITopicService, TopicService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.MapServiceBusEndpoints();

app.Run();
