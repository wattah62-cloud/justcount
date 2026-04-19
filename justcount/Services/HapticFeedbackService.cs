using Microsoft.Maui.Devices;

namespace justcount.Services;

public static class HapticFeedbackService
{
    public static void PerformButtonPress()
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
        }
        catch
        {
        }
    }
}