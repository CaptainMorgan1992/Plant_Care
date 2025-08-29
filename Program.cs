using Auth0_Blazor.Data;
using Auth0_Blazor.Jobs;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services;
using Auth0.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Quartz;

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
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddScoped<ReminderLogicService>();

// This part is responsible for _when_ the job is executing
builder.Services.AddQuartz(q =>
{
    // Remove or comment out NotificationJob registration:
    // var jobKey = new JobKey("NotificationJob");
    // q.AddJob<NotificationJob>(opts => opts.WithIdentity(jobKey));
    // q.AddTrigger(opts => opts
    //     .ForJob(jobKey)
    //     .WithIdentity("NotificationJob-trigger")
    //     .WithSimpleSchedule(x => x
    //         .WithIntervalInSeconds(20)
    //         .RepeatForever()));

    // Add WateringNotificationJob registration:
    var wateringJobKey = new JobKey("WateringNotificationJob");
    q.AddJob<WateringNotificationJob>(opts => opts.WithIdentity(wateringJobKey));
    q.AddTrigger(opts => opts
        .ForJob(wateringJobKey)
        .WithIdentity("WateringNotificationJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(20)
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

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

