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
using MediatR;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    
    var dbConnexion = dbContext.Database.GetDbConnection();
    dbConnexion.Open();
    using (var command = dbConnexion.CreateCommand())
    {
        command.CommandText = "DELETE FROM __EFMigrationsLock";
        command.ExecuteNonQuery();
    }
    dbContext.Database.Migrate();
    
    var relayService = scope.ServiceProvider.GetRequiredService<WebSocketRelayHostedService>();
    await relayService.StartAsync(CancellationToken.None);
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