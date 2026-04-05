using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using justcount.Services;
using Microsoft.Extensions.DependencyInjection;

namespace justcount.Pages;

public partial class HomePage : ContentPage, INotifyPropertyChanged
{
    private readonly ExpenseDatabaseService _expenseDatabaseService;
    private string _monthlyExpenseText = "$0.00";
    private string _monthlyEntryCountText = "0";

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentMonthText { get; } =
        DateTime.Now.ToString("MMMM yyyy", new CultureInfo("en-US"));

    public string MonthlyExpenseText
    {
        get => _monthlyExpenseText;
        set
        {
            if (_monthlyExpenseText == value)
            {
                return;
            }

            _monthlyExpenseText = value;
            RaisePropertyChanged();
        }
    }

    public string MonthlyEntryCountText
    {
        get => _monthlyEntryCountText;
        set
        {
            if (_monthlyEntryCountText == value)
            {
                return;
            }

            _monthlyEntryCountText = value;
            RaisePropertyChanged();
        }
    }

    public HomePage()
    {
        InitializeComponent();
        _expenseDatabaseService = IPlatformApplication.Current!.Services.GetRequiredService<ExpenseDatabaseService>();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshSummaryAsync();
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ExpensePage));
    }

    private async Task RefreshSummaryAsync()
    {
        var today = DateTime.Today;
        MonthlyExpenseText = (await _expenseDatabaseService.GetMonthlyTotalAsync(today)).ToString("C2");
        MonthlyEntryCountText = (await _expenseDatabaseService.GetMonthlyEntryCountAsync(today)).ToString();
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
