using PurpleExplorer.Api.Services;
using PurpleExplorer.Core.Configuration;
using PurpleExplorer.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
