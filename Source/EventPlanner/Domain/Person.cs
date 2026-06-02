namespace EventPlanner.Domain;

public abstract class Person : IValidatable
{
    public int PersonId { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public PersonRole Role { get; protected set; }
    public bool IsActive { get; set; } = true;

    public virtual List<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(FullName)) errors.Add("Full name is required.");
        if (string.IsNullOrWhiteSpace(Phone)) errors.Add("Phone number is required.");
        if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains('@')) errors.Add("Email format is not valid.");
        return errors;
    }
}

public class Attendee : Person
{
    public string Notes { get; set; } = "";

    public Attendee() { Role = PersonRole.Attendee; }
}

public class Speaker : Person
{
    public string Topic { get; set; } = "";
    public string Bio { get; set; } = "";

    public Speaker() { Role = PersonRole.Speaker; }

    public override List<string> Validate()
    {
        var errors = base.Validate();
        if (string.IsNullOrWhiteSpace(Topic)) errors.Add("Speaker topic is required.");
        return errors;
    }
}
