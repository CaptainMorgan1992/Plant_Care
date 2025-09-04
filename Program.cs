using Auth0_Blazor.Data;
using Auth0_Blazor.Enums;
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
builder.Services.AddScoped<UserStateService>();


/*
 * Q U A R T Z  S E T U P
 *  - This part is responsible for _when_ the job is executing.
 * Uses the WateringFrequenzy-enum to determine the interval.
 */

builder.Services.AddQuartz(q =>
{
    var scheduleConfig = new[]
    {
        // EnumValue, IntervalInSeconds
        (WaterFrequency.Low,    60),  // 1 min
        (WaterFrequency.Normal, 30),  // 30 sek
        (WaterFrequency.High,   15)   // 15 sek
    };

    foreach (var (frequency, intervalSeconds) in scheduleConfig)
    {
        var jobKey = new Quartz.JobKey($"WateringNotificationJob-{frequency}");
        q.AddJob<WateringNotificationJob>(opts => opts
            .WithIdentity(jobKey)
            .UsingJobData("frequency", frequency.ToString()));

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{jobKey}-trigger")
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(intervalSeconds)
                .RepeatForever()));
    }
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

