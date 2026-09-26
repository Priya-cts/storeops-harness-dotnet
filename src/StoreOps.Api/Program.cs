using StoreOps.Api.Modules.Activities;
using StoreOps.Api.Modules.Alerts;
using StoreOps.Api.Modules.Programmes;
using StoreOps.Api.Modules.Reports;
using StoreOps.Api.Modules.Staff;
using StoreOps.Api.Shared.Events;
using StoreOps.Api.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Event bus — singleton so subscriptions registered at startup persist for the
// lifetime of the app.
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Activities
builder.Services.AddSingleton<IActivityRepository, InMemoryActivityRepository>();
builder.Services.AddScoped<IActivityService, ActivityService>();

// Programmes
builder.Services.AddSingleton<IProgrammeRepository, InMemoryProgrammeRepository>();
builder.Services.AddScoped<IProgrammeService, ProgrammeService>();

// Staff (auth-only, no controller)
builder.Services.AddSingleton<IStaffRepository, InMemoryStaffRepository>();

// Alerts
builder.Services.AddSingleton<IAlertRepository, InMemoryAlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();

// Reports (read-only aggregator, no controller in this demonstration run)
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Cross-module wiring happens through the event bus only — Alerts subscribes to
// Activities events here, it never references IActivityRepository or ActivityService.
using (var scope = app.Services.CreateScope())
{
    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
    var alertRepository = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
    AlertEventSubscriptions.Register(eventBus, alertRepository);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

// Required so WebApplicationFactory<Program> can be used from the test project.
public partial class Program { }
