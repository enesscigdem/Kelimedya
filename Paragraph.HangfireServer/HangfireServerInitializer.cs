using System.ComponentModel.Design;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Paragraph.HangfireServer;

public static class HangfireServerInitializer
{
    public static IApplicationBuilder AttachMyHangfireJobs(
        this IApplicationBuilder app,
        IRecurringJobManager? recurringJobManager = null)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        HangfireServiceCollectionExtensions.ThrowIfNotConfigured(app.ApplicationServices);

        if (recurringJobManager == null)
        {
            recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
        }

        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 1 });

        var options = new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.Local,
            MisfireHandling = MisfireHandlingMode.Relaxed
        };

        /*recurringJobManager.AddOrUpdate<IReservationReminder>(JOBNAMES.RESERVATION_PAYMENT_REMINDER,
            QUEUENAMES.RECURRING,
            x => x.CheckUnpaidReservationsAndFireRemindersForAgencies(), "0 7 * * *", options);*/
        return app;
    }
}