// Staj.API/Program.cs
using Microsoft.AspNetCore.Identity;
using Serilog;
using Staj.API.Extensions;
using Staj.API.Middleware;
using Staj.Application;
using Staj.Domain.Entities;
using Staj.Infrastructure;
using Staj.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
});

// Katman servisleri
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

// SignalR ve bildirim servisleri
builder.Services.AddSignalR();
builder.Services.AddScoped<Staj.Application.Common.Interfaces.IChatNotifier,
    Staj.Infrastructure.Services.ChatNotifier<Staj.API.Hubs.ChatHub>>();
builder.Services.AddScoped<Staj.Application.Common.Interfaces.INotificationService,
    Staj.Infrastructure.Services.NotificationService<Staj.API.Hubs.ChatHub>>();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Migration ve seed
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    try
    {
        var db = sp.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        await DbInitializer.SeedAsync(db, roleManager, userManager);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Veritabanı başlatılırken hata oluştu.");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<Staj.API.Hubs.ChatHub>("/hubs/chat");

app.Run();