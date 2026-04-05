using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using justcount.Models;
using justcount.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;

namespace justcount.Pages;

public partial class StatsPage : ContentPage, INotifyPropertyChanged
{
    private const string MonthlyBudgetKey = "monthly_budget";
    private readonly ExpenseDatabaseService _expenseDatabaseService;
    private readonly DonutChartDrawable _chartDrawable = new();
    private decimal _monthlyExpense;
    private decimal _totalBudget;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CategorySummary> CategorySummaries { get; } = [];

    public string TotalBudgetText => _totalBudget.ToString("C2");

    public string MonthlyExpenseText => _monthlyExpense.ToString("C2");

    public string FinalBalanceText => (_totalBudget - _monthlyExpense).ToString("C2");

    public Color FinalBalanceColor => _totalBudget - _monthlyExpense >= 0 ? Color.FromArgb("#4E9B58") : Color.FromArgb("#C44D4D");

    public StatsPage()
    {
        InitializeComponent();
        _expenseDatabaseService = IPlatformApplication.Current!.Services.GetRequiredService<ExpenseDatabaseService>();
        BindingContext = this;
        ChartView.Drawable = _chartDrawable;
        _totalBudget = GetBudgetPreference();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMonthlyAnalysisAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//home");
    }

    private async void OnEditBudgetClicked(object sender, EventArgs e)
    {
        var result = await DisplayPromptAsync(
            "Edit budget",
            "Enter your monthly budget",
            initialValue: _totalBudget.ToString("0.##"),
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        if (!decimal.TryParse(result, out var budget) || budget < 0)
        {
            await DisplayAlert("Invalid budget", "Please enter a valid number.", "OK");
            return;
        }

        _totalBudget = budget;
        Preferences.Default.Set(MonthlyBudgetKey, (double)budget);
        RaiseAllDisplayProperties();
    }

    private async Task LoadMonthlyAnalysisAsync()
    {
        var expenses = await _expenseDatabaseService.GetMonthlyExpensesAsync(DateTime.Today);
        _monthlyExpense = expenses.Sum(x => x.Amount);

        var categoryColors = CreateCategoryColors();
        var grouped = expenses
            .GroupBy(x => x.Category)
            .Select(group => new CategorySummary
            {
                Category = group.Key,
                Amount = group.Sum(item => item.Amount),
                DotColor = categoryColors.TryGetValue(group.Key, out var color) ? color : Color.FromArgb("#A0A0A0")
            })
            .OrderByDescending(item => item.Amount)
            .ToList();

        CategorySummaries.Clear();

        foreach (var summary in grouped)
        {
            CategorySummaries.Add(summary);
        }

        if (CategorySummaries.Count == 0)
        {
            foreach (var pair in categoryColors)
            {
                CategorySummaries.Add(new CategorySummary
                {
                    Category = pair.Key,
                    Amount = 0m,
                    DotColor = pair.Value
                });
            }
        }

        _chartDrawable.SetSegments(CategorySummaries.Select(item => new DonutSegment(item.Amount, item.DotColor)).ToList());
        ChartView.Invalidate();
        RaiseAllDisplayProperties();
    }

    private decimal GetBudgetPreference()
    {
        var savedBudget = Preferences.Default.Get(MonthlyBudgetKey, 300d);
        return (decimal)savedBudget;
    }

    private Dictionary<string, Color> CreateCategoryColors()
    {
        return new Dictionary<string, Color>
        {
            ["Entertainment"] = Color.FromArgb("#7B2BD3"),
            ["Health"] = Color.FromArgb("#FF8A2B"),
            ["Shopping"] = Color.FromArgb("#E93776"),
            ["Food"] = Color.FromArgb("#24C05A"),
            ["Others"] = Color.FromArgb("#37A8E4"),
            ["Transportation"] = Color.FromArgb("#FF4D6D")
        };
    }

    private void RaiseAllDisplayProperties()
    {
        RaisePropertyChanged(nameof(TotalBudgetText));
        RaisePropertyChanged(nameof(MonthlyExpenseText));
        RaisePropertyChanged(nameof(FinalBalanceText));
        RaisePropertyChanged(nameof(FinalBalanceColor));
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class DonutChartDrawable : IDrawable
    {
        private IReadOnlyList<DonutSegment> _segments = [];

        public void SetSegments(IReadOnlyList<DonutSegment> segments)
        {
            _segments = segments;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.Antialias = true;

            var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - 20;
            var strokeSize = size * 0.2f;
            var radius = size / 2;
            var centerX = dirtyRect.Center.X;
            var centerY = dirtyRect.Center.Y;
            var arcRect = new RectF(centerX - radius, centerY - radius, size, size);
            var total = _segments.Sum(segment => segment.Amount);

            canvas.StrokeColor = Color.FromArgb("#F3E8DA");
            canvas.StrokeSize = strokeSize;
            canvas.DrawArc(arcRect, 0, 360, false, false);

            if (total <= 0)
            {
                canvas.RestoreState();
                return;
            }

            var startAngle = -90f;
            foreach (var segment in _segments.Where(segment => segment.Amount > 0))
            {
                var sweepAngle = (float)(segment.Amount / total * 360m);
                canvas.StrokeColor = segment.Color;
                canvas.StrokeSize = strokeSize;
                canvas.DrawArc(arcRect, startAngle, startAngle + sweepAngle - 3, false, false);
                startAngle += sweepAngle;
            }

            canvas.RestoreState();
        }
    }

    private sealed record DonutSegment(decimal Amount, Color Color);

    public sealed class CategorySummary
    {
        public string Category { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public Color DotColor { get; init; } = Colors.Transparent;
    }
}

