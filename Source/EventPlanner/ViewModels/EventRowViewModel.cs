using EventPlanner.Domain;

namespace EventPlanner.ViewModels;

public class EventRowViewModel
{
    public Event Event { get; }

    public int EventId => Event.EventId;
    public string Title => Event.Title;
    public DateTime StartAt => Event.StartAt;
    public string Location => Event.Location;
    public int Capacity => Event.Capacity;
    public EventStatus Status => Event.Status;

    public int Registered { get; }
    public decimal Paid { get; }

    public EventRowViewModel(Event ev, IEnumerable<Registration> allRegistrations)
    {
        Event = ev;
        var forEvent = allRegistrations.Where(r => r.EventId == ev.EventId && r.Status != PaymentStatus.Cancelled).ToList();
        Registered = forEvent.Count;
        Paid = forEvent.Sum(r => r.PaidAmount);
    }
}
