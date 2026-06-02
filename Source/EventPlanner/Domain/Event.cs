namespace EventPlanner.Domain;

public class Event : IValidatable
{
    public int EventId { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartAt { get; set; } = DateTime.Now;
    public string Location { get; set; } = "";
    public int Capacity { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public string Description { get; set; } = "";

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Title)) errors.Add("Event title is required.");
        if (string.IsNullOrWhiteSpace(Location)) errors.Add("Event location is required.");
        if (Capacity <= 0) errors.Add("Capacity must be greater than zero.");
        return errors;
    }
}
