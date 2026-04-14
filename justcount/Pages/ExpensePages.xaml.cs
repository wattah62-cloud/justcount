using System.Globalization;
using justcount.Models;
using justcount.Services;
using Microsoft.Extensions.DependencyInjection;

namespace justcount.Pages;

public partial class ExpensePage : ContentPage
{
    private readonly ExpenseDatabaseService _expenseDatabaseService;

    public ExpensePage()
    {
        InitializeComponent();
        _expenseDatabaseService = IPlatformApplication.Current!.Services.GetRequiredService<ExpenseDatabaseService>();

        CategoryPicker.ItemsSource = new List<string>
        {
            "Food",
            "Transportation",
            "Shopping",
            "Entertainment",
            "Health",
            "Others"
        };

        ExpenseDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshExpensesForSelectedDate();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnExpenseDateSelected(object sender, DateChangedEventArgs e)
    {
        await RefreshExpensesForSelectedDate();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        HapticFeedbackService.PerformButtonPress();

        if (CategoryPicker.SelectedItem is not string category)
        {
            ShowStatus("Please choose a category.", false);
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            ShowStatus("Please enter a valid amount.", false);
            return;
        }

        var item = new ExpenseItem
        {
            Category = category,
            Amount = amount,
            Date = ExpenseDatePicker.Date ?? DateTime.Today,
            Notes = NotesEditor.Text?.Trim() ?? string.Empty
        };

        await _expenseDatabaseService.AddExpenseAsync(item);

        AmountEntry.Text = string.Empty;
        NotesEditor.Text = string.Empty;
        CategoryPicker.SelectedItem = null;

        ShowStatus("Expense added.", true);
        await RefreshExpensesForSelectedDate();
    }

    private async Task RefreshExpensesForSelectedDate()
    {
        var selectedDate = (ExpenseDatePicker.Date ?? DateTime.Today).Date;
        var filtered = await _expenseDatabaseService.GetExpensesByDateAsync(selectedDate);

        AddedExpensesView.ItemsSource = filtered;
        SelectedDateLabel.Text = selectedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        TotalAmountLabel.Text = filtered.Sum(x => x.Amount).ToString("C2");
    }

    private void ShowStatus(string message, bool isSuccess)
    {
        StatusLabel.Text = message;
        StatusLabel.TextColor = isSuccess ? Colors.Green : Colors.Red;
        StatusLabel.IsVisible = true;
    }
}
