
using aurastrip_adapter.Contexts;
using aurastrip_adapter.Controllers;
using aurastrip_adapter.Repositories.Column;
using aurastrip_adapter.Repositories.Configuration;
using aurastrip_adapter.Repositories.Slot;
using aurastrip_adapter.Repositories.Strip;
using aurastrip_adapter.Services;
using aurastrip_adapter.Services.Repositories.Configuration;
using aurastrip_adapter.WebSockets;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ConfigurationDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<ColumnService>();
builder.Services.AddScoped<SlotService>();
builder.Services.AddScoped<StripService>();

// Repositories
builder.Services.AddScoped<IConfigurationRepository, LocalConfigurationRespository>();
builder.Services.AddScoped<IColumnRepository, LocalColumnRepository>();
builder.Services.AddScoped<ISlotRepository, LocalSlotRepository>();
builder.Services.AddScoped<IStripRepository, LocalStripRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    dbContext.Database.EnsureCreated();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/configurations")
    .MapConfigurationEndpoints();
app.MapGroup("/columns")
    .MapColumnEndpoints();
app.MapGroup("/strips")
    .MapStripEndpoints();

WebSocketRelayServer.Start();

app.Run();