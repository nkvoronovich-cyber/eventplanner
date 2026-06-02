namespace EventPlanner.Domain;

public interface IValidatable
{
    List<string> Validate();
}
