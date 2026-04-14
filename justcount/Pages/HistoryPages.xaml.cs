using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using justcount.Models;
using justcount.Services;
using Microsoft.Extensions.DependencyInjection;

namespace justcount.Pages;

public partial class HistoryPage : ContentPage, INotifyPropertyChanged
{
    private static readonly string[] Categories =
    [
        "Food",
        "Transportation",
        "Shopping",
        "Entertainment",
        "Health",
        "Others"
    ];

    private readonly ExpenseDatabaseService _expenseDatabaseService;
    private DateTime _selectedDate = DateTime.Today;
    private DateTime _displayMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public new event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CalendarDayCell> CalendarDays { get; } = [];

    public string DisplayMonthText => _displayMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    public HistoryPage()
    {
        InitializeComponent();
        _expenseDatabaseService = IPlatformApplication.Current!.Services.GetRequiredService<ExpenseDatabaseService>();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshCalendarAndExpensesAsync();
    }

    private async void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        _displayMonth = _displayMonth.AddMonths(-1);
        _selectedDate = CreateDateInDisplayMonth(_selectedDate.Day);
        await RefreshCalendarAndExpensesAsync();
    }

    private async void OnNextMonthClicked(object sender, EventArgs e)
    {
        _displayMonth = _displayMonth.AddMonths(1);
        _selectedDate = CreateDateInDisplayMonth(_selectedDate.Day);
        await RefreshCalendarAndExpensesAsync();
    }

    private async void OnCalendarDayClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: CalendarDayCell day } || !day.IsSelectable)
        {
            return;
        }

        _selectedDate = day.Date;
        _displayMonth = new DateTime(day.Date.Year, day.Date.Month, 1);
        await RefreshCalendarAndExpensesAsync(clearStatus: true);
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        var selectedCategory = await DisplayActionSheetAsync(
            "Select category",
            "Cancel",
            null,
            Categories);

        if (string.IsNullOrWhiteSpace(selectedCategory) || selectedCategory == "Cancel")
        {
            return;
        }

        var amountInput = await DisplayPromptAsync(
            "Add expense",
            $"Enter the amount for {_selectedDate:dd MMM yyyy}",
            keyboard: Keyboard.Numeric);

        if (amountInput is null)
        {
            return;
        }

        if (!decimal.TryParse(amountInput, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ShowStatus("Please enter a valid amount before saving.", false);
            return;
        }

        var noteInput = await DisplayPromptAsync(
            "Add notes",
            "Add optional notes for this expense",
            initialValue: string.Empty,
            maxLength: 200);

        if (noteInput is null)
        {
            return;
        }

        await _expenseDatabaseService.AddExpenseAsync(new ExpenseItem
        {
            Category = selectedCategory,
            Amount = amount,
            Date = _selectedDate,
            Notes = noteInput.Trim()
        });

        await RefreshCalendarAndExpensesAsync(clearStatus: false);
        ShowStatus("Expense added.", true);
    }

    private async void OnModifyClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: ExpenseItem item })
        {
            return;
        }

        var selectedCategory = await DisplayActionSheetAsync(
            "Select category",
            "Cancel",
            null,
            Categories);

        if (selectedCategory == "Cancel")
        {
            selectedCategory = item.Category;
        }

        var amountInput = await DisplayPromptAsync(
            "Modify amount",
            "Enter the updated expense amount",
            initialValue: item.Amount.ToString("0.##", CultureInfo.InvariantCulture),
            keyboard: Keyboard.Numeric);

        if (amountInput is null)
        {
            return;
        }

        if (!decimal.TryParse(amountInput, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ShowStatus("Please enter a valid amount before saving.", false);
            return;
        }

        var noteInput = await DisplayPromptAsync(
            "Modify notes",
            "Update the notes for this expense",
            initialValue: item.Notes,
            maxLength: 200);

        if (noteInput is null)
        {
            return;
        }

        item.Category = selectedCategory;
        item.Amount = amount;
        item.Notes = noteInput.Trim();
        item.Date = _selectedDate;

        await _expenseDatabaseService.UpdateExpenseAsync(item);
        await RefreshCalendarAndExpensesAsync(clearStatus: false);
        ShowStatus("Expense updated.", true);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: ExpenseItem item })
        {
            return;
        }

        var shouldDelete = await DisplayAlertAsync(
            "Delete expense",
            $"Delete {item.Category} expense of {item.AmountText}?",
            "Delete",
            "Cancel");

        if (!shouldDelete)
        {
            return;
        }

        await _expenseDatabaseService.DeleteExpenseAsync(item);
        await RefreshCalendarAndExpensesAsync(clearStatus: false);
        ShowStatus("Expense deleted.", true);
    }

    private async Task RefreshCalendarAndExpensesAsync(bool clearStatus = true)
    {
        await RefreshCalendarAsync();
        await RefreshExpensesForSelectedDateAsync(clearStatus);
    }

    private async Task RefreshCalendarAsync()
    {
        var monthExpenses = await _expenseDatabaseService.GetMonthlyExpensesAsync(_displayMonth);
        var markedDates = monthExpenses
            .Select(item => item.Date.Date)
            .ToHashSet();

        var firstDayOfMonth = new DateTime(_displayMonth.Year, _displayMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month);
        var leadingBlankDays = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

        CalendarDays.Clear();

        for (var i = 0; i < leadingBlankDays; i++)
        {
            CalendarDays.Add(CalendarDayCell.CreatePlaceholder());
        }

        for (var day = 1; day <= daysInMonth; day++)
        {
            var currentDate = new DateTime(_displayMonth.Year, _displayMonth.Month, day);
            var isSelected = currentDate.Date == _selectedDate.Date;
            var hasExpenses = markedDates.Contains(currentDate.Date);

            CalendarDays.Add(new CalendarDayCell
            {
                Date = currentDate,
                DayText = day.ToString(CultureInfo.InvariantCulture),
                IsSelectable = true,
                HasExpenses = hasExpenses,
                BackgroundColor = isSelected ? Color.FromArgb("#D9C2A3") : Color.FromArgb("#FFF9F2"),
                TextColor = isSelected ? Colors.White : Color.FromArgb("#3E372F"),
                MarkerColor = hasExpenses ? Color.FromArgb("#C44D4D") : Colors.Transparent
            });
        }

        while (CalendarDays.Count % 7 != 0)
        {
            CalendarDays.Add(CalendarDayCell.CreatePlaceholder());
        }

        RaisePropertyChanged(nameof(DisplayMonthText));
    }

    private async Task RefreshExpensesForSelectedDateAsync(bool clearStatus)
    {
        var expenses = await _expenseDatabaseService.GetExpensesByDateAsync(_selectedDate);

        HistoryExpensesView.ItemsSource = expenses;
        SelectedDateLabel.Text = _selectedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        TotalAmountLabel.Text = expenses.Sum(item => item.Amount).ToString("C2");

        if (clearStatus)
        {
            StatusLabel.IsVisible = false;
        }
    }

    private void ShowStatus(string message, bool isSuccess)
    {
        StatusLabel.Text = message;
        StatusLabel.TextColor = isSuccess ? Colors.Green : Colors.Red;
        StatusLabel.IsVisible = true;
    }

    private DateTime CreateDateInDisplayMonth(int preferredDay)
    {
        var day = Math.Min(preferredDay, DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month));
        return new DateTime(_displayMonth.Year, _displayMonth.Month, day);
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public sealed class CalendarDayCell
    {
        public DateTime Date { get; init; }

        public string DayText { get; init; } = string.Empty;

        public bool IsSelectable { get; init; }

        public bool HasExpenses { get; init; }

        public Color BackgroundColor { get; init; } = Colors.Transparent;

        public Color TextColor { get; init; } = Colors.Transparent;

        public Color MarkerColor { get; init; } = Colors.Transparent;

        public static CalendarDayCell CreatePlaceholder()
        {
            return new CalendarDayCell
            {
                IsSelectable = false,
                DayText = string.Empty,
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Transparent,
                MarkerColor = Colors.Transparent
            };
        }
    }
}
