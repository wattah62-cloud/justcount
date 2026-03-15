using justcount.Models;

namespace justcount.Services;

public sealed class ExpenseService
{
    private readonly List<ExpenseItem> _expenses = [];

    public event Action? ExpensesChanged;

    public void AddExpense(ExpenseItem item)
    {
        _expenses.Add(item);
        ExpensesChanged?.Invoke();
    }

    public List<ExpenseItem> GetExpensesByDate(DateTime date)
    {
        return _expenses
            .Where(x => x.Date.Date == date.Date)
            .OrderByDescending(x => x.Date)
            .ToList();
    }

    public decimal GetMonthlyTotal(DateTime date)
    {
        return _expenses
            .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month)
            .Sum(x => x.Amount);
    }

    public int GetMonthlyEntryCount(DateTime date)
    {
        return _expenses.Count(x => x.Date.Year == date.Year && x.Date.Month == date.Month);
    }
}