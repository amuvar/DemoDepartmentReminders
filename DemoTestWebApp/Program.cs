using DemoTestWebApp.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Configure Email Settings
builder.Services.AddSingleton<SmtpClient>(sp =>
{
    var settings = builder.Configuration.GetSection("EmailSettings");
    return new SmtpClient
    {
        Host = settings["Host"],
        Port = int.Parse(settings["Port"]),
        Credentials = new NetworkCredential(settings["Username"], settings["Password"]),
        EnableSsl = true
    };
});


builder.Services.AddScoped<IEmailSender, EmailSender>();
//builder.Services.AddSingleton<ReminderService>(); // Keep as Singleton
//builder.Services.AddHostedService<ReminderService>(); // Add as hosted service



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Departments}/{action=Index}/{id?}");

app.Run();
