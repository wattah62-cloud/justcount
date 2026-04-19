using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace justcount.Services;

public sealed class ReminderNotificationService
{
    private const int TodaySummaryNotificationId = 9301;
    private readonly ExpenseDatabaseService _expenseDatabaseService;

    public ReminderNotificationService(ExpenseDatabaseService expenseDatabaseService)
    {
        _expenseDatabaseService = expenseDatabaseService;
    }

    public async Task EnsurePermissionAsync()
    {
        var permission = new NotificationPermission();

#if ANDROID
        permission.Android.RequestPermissionToScheduleExactAlarm = true;
#endif

        var isEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled(permission);

        if (!isEnabled)
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission(permission);
        }
    }

    public async Task RefreshTodaySummaryNotificationAsync()
    {
        LocalNotificationCenter.Current.Cancel(TodaySummaryNotificationId);

        var today = DateTime.Today;
        var notifyTime = today.AddHours(21).AddMinutes(30);

        if (DateTime.Now >= notifyTime)
        {
            return;
        }

        var expenses = await _expenseDatabaseService.GetExpensesByDateAsync(today);
        var total = expenses.Sum(x => x.Amount);

        var request = new NotificationRequest
        {
            NotificationId = TodaySummaryNotificationId,
            Title = "Today expense summary",
            Description = expenses.Count == 0
                ? "No expense logged today. Remember to log your expenses!"
                : $"Today total expense is {total:C2} across {expenses.Count} entries.",
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime
            }
        };

        await LocalNotificationCenter.Current.Show(request);
    }
}
