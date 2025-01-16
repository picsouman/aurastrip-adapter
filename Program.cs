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

// Repositories
builder.Services.AddScoped<IConfigurationRepository, LocalConfigurationRespository>();
builder.Services.AddScoped<IColumnRepository, LocalColumnRepository>();
builder.Services.AddScoped<ISlotRepository, LocalSlotRepository>();
builder.Services.AddScoped<IStripRepository, LocalStripRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
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

WebSocketRelayServer.Start(app.Services.GetRequiredService<IMediator>());

app.Run();