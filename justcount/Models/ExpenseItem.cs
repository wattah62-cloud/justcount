using SQLite;

namespace justcount.Models;

public sealed class ExpenseItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; } = string.Empty;

    [Ignore]
    public string AmountText => Amount.ToString("C2");

    [Ignore]
    public string NotesText => string.IsNullOrWhiteSpace(Notes) ? "No notes" : Notes;
}
