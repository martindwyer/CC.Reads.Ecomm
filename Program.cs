/*
Filename:  Program.cs
Purpose:   To configure and build the application  
Contains:  Configuration for Db connection, API dependencies, Routing, Authentication, Authorization
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

using CC.Reads.DataAccess.Data;
using CC.Reads.DataAccess.Repository;
using CC.Reads.DataAccess.Repository.Interface;
using CC.Reads.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

// Step 1: Create builder object
var builder = WebApplication.CreateBuilder(args);

// Step 2: Create services with builder object

// Creating Database Service
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 29));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Creating services for Controllers and Views
builder.Services.AddControllersWithViews();

// Set up Stripe API for payment and refund processing
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Stripe configuration 
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

// Adding the .NET Identity User and Manager with database context
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();

// Implement interface for data repository service
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Create email service
builder.Services.AddSingleton<IEmailSender, EmailSender>();

// Adding Razor pages and Razor runtime compilation
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Creating service for Facebook social media authentication 
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = builder.Configuration.GetSection("Facebook")["AppId"];
    options.AppSecret = builder.Configuration.GetSection("Facebook")["AppSecret"];
});

// Creating paths for login, logout and access denied (Identity model pages)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// Creating services for managing user session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Build the app with services defined
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// For https redirection and routing
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Direction to use authentication and authorization (order important)
app.UseAuthentication(); ;
app.UseAuthorization();
app.UseSession();

// Adding Razor Page support
app.MapRazorPages();

// Specifying the application default route
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Landing}/{id?}");

// Run the application 
app.Run();

/**
* Public url: https://cc-reads-app-jmupf.ondigitalocean.app/
*/