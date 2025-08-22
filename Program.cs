using Auth0_Blazor.Data;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services;
using Auth0.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"] 
                     ?? throw new InvalidOperationException("Auth0:Domain not configured");

    options.ClientId = builder.Configuration["Auth0:ClientId"]
                       ?? throw new InvalidOperationException("Auth0:ClientId not configured");
    options.Scope = "openid profile email";
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddMudServices();

builder.Services.AddScoped<PlantService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UtilityService>();
builder.Services.AddScoped<UserPlantService>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<TokenProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// You want this to be enabled in your production applications.
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

