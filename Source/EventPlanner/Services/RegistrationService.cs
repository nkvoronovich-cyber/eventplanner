using EventPlanner.Domain;
using EventPlanner.Repositories;

namespace EventPlanner.Services;

public class RegistrationService
{
    private readonly RegistrationRepository _registrations;

    public RegistrationService(RegistrationRepository registrations)
    {
        _registrations = registrations;
    }

    public void EnsureCapacity(Event selectedEvent)
    {
        var activeCount = _registrations.GetByEventId(selectedEvent.EventId)
            .Count(r => r.Status != PaymentStatus.Cancelled);
        if (activeCount >= selectedEvent.Capacity)
            throw new InvalidOperationException("Event capacity has been reached.");
    }

    public void EnsureNotAlreadyRegistered(int eventId, int personId)
    {
        var existing = _registrations.GetByEventId(eventId)
            .Any(r => r.PersonId == personId && r.Status != PaymentStatus.Cancelled);
        if (existing)
            throw new InvalidOperationException("This person is already registered for the selected event.");
    }

    public Registration CreateRegistration(Event selectedEvent, int personId, string ticketType, decimal price, decimal paidAmount)
    {
        EnsureCapacity(selectedEvent);
        EnsureNotAlreadyRegistered(selectedEvent.EventId, personId);

        var registration = new Registration
        {
            EventId = selectedEvent.EventId,
            PersonId = personId,
            TicketType = ticketType,
            CreatedAt = DateTime.Now
        };
        registration.UpdatePayment(price, paidAmount);

        var errors = registration.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));
        return registration;
    }
}
