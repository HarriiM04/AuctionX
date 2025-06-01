using System.Text.Json.Serialization;
using AuctionX.Hubs;
using AuctionX.Models;
using AuctionX.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

//for security credential accessing
DotNetEnv.Env.Load();  // Loads .env into Environment variables

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

Console.WriteLine("Google ClientId: " + builder.Configuration["Authentication:Google:ClientId"]);
Console.WriteLine("Google ClientSecret: " + builder.Configuration["Authentication:Google:ClientSecret"]);

Console.WriteLine("Current Directory: " + Directory.GetCurrentDirectory());


// Configure DbContext
builder.Services.AddDbContext<SportsAuctionContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBCS")));

// Configure Email Settings and Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();


// Add SignalR
// Add before builder.Build()
builder.Services.AddSignalR();


// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
    IConfigurationSection googleAuth = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleAuth["ClientId"];
    options.ClientSecret = googleAuth["ClientSecret"];
    options.ClaimActions.MapJsonKey("picture", "picture", "url");
});

// Configure authorization
builder.Services.AddAuthorization();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseDeveloperExceptionPage();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

// Update your app configuration:
app.MapHub<AuctionHub>("/auctionHub", options => {
    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
});

app.Run();
