using System.Globalization;
using justcount.Models;
using justcount.Services;

namespace justcount.Pages;

public partial class ExpensePage : ContentPage
{
    private readonly ExpenseService _expenseService;

    public ExpensePage()
    {
        InitializeComponent();
        _expenseService = AppServices.ExpenseService;

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
        RefreshExpensesForSelectedDate();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnExpenseDateSelected(object sender, DateChangedEventArgs e)
    {
        RefreshExpensesForSelectedDate();
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
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

        _expenseService.AddExpense(item);

        AmountEntry.Text = string.Empty;
        NotesEditor.Text = string.Empty;
        CategoryPicker.SelectedItem = null;

        ShowStatus("Expense added.", true);
        RefreshExpensesForSelectedDate();
    }

    private void RefreshExpensesForSelectedDate()
    {
        var selectedDate = (ExpenseDatePicker.Date ?? DateTime.Today).Date;
        var filtered = _expenseService.GetExpensesByDate(selectedDate);

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
