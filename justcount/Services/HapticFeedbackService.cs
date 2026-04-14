using Microsoft.Maui.Devices;

namespace justcount.Services;

public static class HapticFeedbackService
{
    public static void PerformButtonPress()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
            TryFallbackVibration();
        }
        catch
        {
         
        }
    }

    private static void TryFallbackVibration()
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(30));
        }
        catch
        {

        }
    }
}
