using AZRedis.Application.Configuration;
using AZRedis.Application.Interfaces;
using AZRedis.Application.Services;
using AZRedis.Infrastructure.Services;
using AzureRedis.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IRedisConnectionStore, JsonRedisConnectionStore>();
builder.Services.AddSingleton<IRedisConnectionProvider, ApiRedisConnectionProvider>();
builder.Services.AddSingleton<IRedisKeyService, RedisKeyService>();
builder.Services.AddSingleton<IRedisDataService, RedisDataService>();
builder.Services.AddSingleton<IRedisServerService, RedisServerService>();
builder.Services.AddSingleton<IRedisConnectionTester, RedisConnectionTester>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapAZRedisEndpoints();

app.Run();
