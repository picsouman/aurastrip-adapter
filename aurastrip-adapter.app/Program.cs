using System.Text.Json;
using aurastrip_adapter.Contexts;
using aurastrip_adapter.Controllers;
using aurastrip_adapter.Endpoints;
using aurastrip_adapter.Repositories.Column;
using aurastrip_adapter.Repositories.Configuration;
using aurastrip_adapter.Repositories.Slot;
using aurastrip_adapter.Repositories.Strip;
using aurastrip_adapter.Services;
using aurastrip_adapter.Services.Repositories.Configuration;
using aurastrip_adapter.WebSockets;
using Microsoft.EntityFrameworkCore;


Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5131);
});

builder.Services.AddDbContext<ConfigurationDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(setup =>
{
    setup.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
    });
});

// Services
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<ColumnService>();
builder.Services.AddScoped<SlotService>();
builder.Services.AddScoped<StripService>();
builder.Services.AddSingleton<AuroraService>();
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddSingleton<WebSocketRelayHostedService>();

builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
});

// Repositories
builder.Services.AddScoped<IConfigurationRepository, LocalConfigurationRespository>();
builder.Services.AddScoped<IColumnRepository, LocalColumnRepository>();
builder.Services.AddScoped<ISlotRepository, LocalSlotRepository>();
builder.Services.AddScoped<IStripRepository, LocalStripRepository>();

// Windows Hosted Services
builder.Services.AddWindowsService();
builder.Services.AddHostedService<WebSocketRelayHostedService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    
    var db = dbContext.Database;
    var lockExists = db.ExecuteSqlRaw("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsLock';");
    if (lockExists > 0)
    {
        db.ExecuteSqlRaw("DELETE FROM __EFMigrationsLock;");
    }
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGroup("/configurations")
    .WithTags("Configurations")
    .MapConfigurationEndpoints();

app.MapGroup("/columns")
    .WithTags("Columns")
    .MapColumnEndpoints();

app.MapGroup("slots")
    .WithTags("Slots")
    .MapSlotEndpoints();

app.MapGroup("/strips")
    .WithTags("Strips")
    .MapStripEndpoints();

app.Run();