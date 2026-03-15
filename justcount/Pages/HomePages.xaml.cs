using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using justcount.Services;

namespace justcount.Pages;

public partial class HomePage : ContentPage, INotifyPropertyChanged
{
    private readonly ExpenseService _expenseService;
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
        _expenseService = AppServices.ExpenseService;
        _expenseService.ExpensesChanged += OnExpensesChanged;
        BindingContext = this;
        RefreshSummary();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshSummary();
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ExpensePage));
    }

    private void OnExpensesChanged()
    {
        MainThread.BeginInvokeOnMainThread(RefreshSummary);
    }

    private void RefreshSummary()
    {
        var today = DateTime.Today;
        MonthlyExpenseText = _expenseService.GetMonthlyTotal(today).ToString("C2");
        MonthlyEntryCountText = _expenseService.GetMonthlyEntryCount(today).ToString();
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
