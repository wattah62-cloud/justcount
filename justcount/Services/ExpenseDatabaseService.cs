using justcount.Models;
using SQLite;

namespace justcount.Services;

public sealed class ExpenseDatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database is not null)
        {
            return;
        }

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "expenses.db3");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<ExpenseItem>();
    }

    public async Task AddExpenseAsync(ExpenseItem item)
    {
        await InitAsync();
        await _database!.InsertAsync(item);
    }

    public async Task<List<ExpenseItem>> GetExpensesByDateAsync(DateTime date)
    {
        await InitAsync();

        var items = await _database!.Table<ExpenseItem>().ToListAsync();

        return items
            .Where(x => x.Date.Date == date.Date)
            .OrderByDescending(x => x.Date)
            .ToList();
    }

    public async Task<decimal> GetMonthlyTotalAsync(DateTime date)
    {
        await InitAsync();

        var items = await _database!.Table<ExpenseItem>().ToListAsync();

        return items
            .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month)
            .Sum(x => x.Amount);
    }

    public async Task<int> GetMonthlyEntryCountAsync(DateTime date)
    {
        await InitAsync();

        var items = await _database!.Table<ExpenseItem>().ToListAsync();

        return items.Count(x => x.Date.Year == date.Year && x.Date.Month == date.Month);
    }

    public async Task<List<ExpenseItem>> GetMonthlyExpensesAsync(DateTime date)
    {
        await InitAsync();

        var items = await _database!.Table<ExpenseItem>().ToListAsync();

        return items
            .Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month)
            .OrderByDescending(x => x.Date)
            .ToList();
    }
}
