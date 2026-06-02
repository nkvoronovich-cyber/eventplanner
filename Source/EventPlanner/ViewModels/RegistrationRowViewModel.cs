using EventPlanner.Domain;

namespace EventPlanner.ViewModels;

public class RegistrationRowViewModel
{
    public Registration Registration { get; }

    public int RegistrationId => Registration.RegistrationId;
    public string EventTitle { get; }
    public string PersonName { get; }
    public string TicketType => Registration.TicketType;
    public decimal Price => Registration.Price;
    public decimal PaidAmount => Registration.PaidAmount;
    public PaymentStatus Status => Registration.Status;
    public DateTime? CheckedInAt => Registration.CheckedInAt;

    public RegistrationRowViewModel(Registration r, string eventTitle, string personName)
    {
        Registration = r;
        EventTitle = eventTitle;
        PersonName = personName;
    }
}
