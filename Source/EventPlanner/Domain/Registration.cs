namespace EventPlanner.Domain;

public class Registration : IValidatable
{
    public int RegistrationId { get; set; }
    public int EventId { get; set; }
    public int PersonId { get; set; }
    public string TicketType { get; set; } = "Standard";
    public decimal Price { get; private set; }
    public decimal PaidAmount { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Unpaid;
    public string Notes { get; set; } = "";
    public DateTime? CheckedInAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public void UpdatePayment(decimal price, decimal paidAmount, bool cancelled = false)
    {
        Price = price;
        PaidAmount = paidAmount;
        Status = ComputeStatus(price, paidAmount, cancelled);
    }

    public void Cancel() => Status = PaymentStatus.Cancelled;

    public void CheckIn()
    {
        if (Status == PaymentStatus.Cancelled)
            throw new InvalidOperationException("Cancelled registration cannot be checked in.");
        CheckedInAt = DateTime.Now;
    }

    public static PaymentStatus ComputeStatus(decimal price, decimal paidAmount, bool cancelled = false)
    {
        // Small tolerance protects against rounding noise from REAL <-> decimal conversion.
        const decimal eps = 0.005m;
        if (cancelled) return PaymentStatus.Cancelled;
        if (price <= eps) return PaymentStatus.Free;
        if (paidAmount <= eps) return PaymentStatus.Unpaid;
        if (paidAmount + eps < price) return PaymentStatus.PartPaid;
        return PaymentStatus.Paid;
    }

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (EventId <= 0) errors.Add("Event must be selected.");
        if (PersonId <= 0) errors.Add("Person must be selected.");
        if (Price < 0) errors.Add("Ticket price cannot be negative.");
        if (PaidAmount < 0) errors.Add("Paid amount cannot be negative.");
        if (Price == 0 && PaidAmount != 0) errors.Add("Free ticket must have paid amount equal to 0.");
        if (Price > 0 && PaidAmount > Price) errors.Add("Paid amount cannot be greater than ticket price.");
        return errors;
    }
}
