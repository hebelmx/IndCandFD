using BlazorApp2.Data;
using Config;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

//"Data Source=test.db";
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "PcCan.db");
var YourConnectionName = $"Data Source={dbPath}";


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString(YourConnectionName);
    
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<IConfigApplication>(provider => new ConfigApplication(YourConnectionName));
builder.Services.AddScoped<IConfigApplication>(provider => new ConfigApplication(YourConnectionName));




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
