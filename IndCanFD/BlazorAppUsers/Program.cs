using System.Data;
using BlazorApp1.Areas.Identity;
using BlazorApp1.Data;
using ConfigDataService;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using FluentAssertions.Common;
using FramesDataService;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Year)
    .WriteTo.Seq("http://localhost:5341",
        controlLevelSwitch: null,
        restrictedToMinimumLevel: LogEventLevel.Verbose,
        apiKey: null,
        batchPostingLimit: 1000,
        period: TimeSpan.FromSeconds(2),
        retainedInvalidPayloadsLimitBytes: null,
        messageHandler: null,
        eventBodyLimitBytes: 262144
        )
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSerilog();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddScoped<IDbConnection>(serviceProvider => new SqlConnection(connectionString));
builder.Services.AddScoped<ICommandLengthService, CommandLengthService>();
builder.Services.AddScoped<ConfigDataValidator>();
builder.Services.AddScoped< IConfigDataService,ConfigDataService.ConfigDataService>();
builder.Services.AddScoped<IDatabaseInitializer>(serviceProvider => new DatabaseInitializer(connectionString));
builder.Services.AddMatBlazor();
builder.Services.AddHttpClient();


builder.Services.AddMatToaster(config =>
{
    config.Position = MatToastPosition.TopCenter;
    config.PreventDuplicates = true;
    config.NewestOnTop = true;
    config.ShowCloseButton = true;
    config.MaximumOpacity = 95;
    config.VisibleStateDuration = 3000;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();


// Close and flush the log
Log.CloseAndFlush();