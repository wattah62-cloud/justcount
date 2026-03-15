namespace justcount.Models;

public sealed class ExpenseItem
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string AmountText => Amount.ToString("C2");
    public string NotesText => string.IsNullOrWhiteSpace(Notes) ? "No notes" : Notes;
}