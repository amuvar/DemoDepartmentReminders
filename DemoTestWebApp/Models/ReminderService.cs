using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DemoTestWebApp.Models
{
    public class ReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public ReminderService(IServiceScopeFactory scopeFactory, ILogger<ReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking for due reminders...");

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    var dueReminders = await context.Reminders
                        .Where(r => r.ReminderDateTime <= DateTime.Now)
                        .ToListAsync();

                    foreach (var reminder in dueReminders)
                    {
                        await emailService.SendEmailAsync("amolpatil019@gmail.com", reminder.Title, "It's time for your reminder!");
                        context.Reminders.Remove(reminder);
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }


}
