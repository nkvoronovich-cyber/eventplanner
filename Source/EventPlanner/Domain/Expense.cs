namespace EventPlanner.Domain;

public class Expense : IValidatable
{
    public int ExpenseId { get; set; }
    public int EventId { get; set; }
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public string Notes { get; set; } = "";
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (EventId <= 0) errors.Add("Event must be selected.");
        if (string.IsNullOrWhiteSpace(Category)) errors.Add("Expense category is required.");
        if (Amount <= 0) errors.Add("Expense amount must be greater than zero.");
        return errors;
    }
}
