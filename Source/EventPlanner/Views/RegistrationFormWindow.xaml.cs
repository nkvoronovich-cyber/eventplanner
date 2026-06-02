using System.Globalization;
using System.Windows;
using EventPlanner.Domain;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class RegistrationFormWindow : Window
{
    public Registration? Result { get; private set; }

    private readonly MainViewModel _vm;
    private readonly Registration? _existing;

    public RegistrationFormWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        foreach (var e in vm.Events) EventBox.Items.Add(e);
        foreach (var p in vm.People.Where(p => p.IsActive)) PersonBox.Items.Add(p);
        if (EventBox.Items.Count > 0) EventBox.SelectedIndex = 0;
        if (PersonBox.Items.Count > 0) PersonBox.SelectedIndex = 0;
    }

    public RegistrationFormWindow(MainViewModel vm, Event preselectedEvent) : this(vm)
    {
        EventBox.SelectedItem = vm.Events.FirstOrDefault(e => e.EventId == preselectedEvent.EventId);
        EventBox.IsEnabled = false;
    }

    public RegistrationFormWindow(MainViewModel vm, Registration existing) : this(vm)
    {
        _existing = existing;
        EventBox.SelectedItem = vm.Events.FirstOrDefault(e => e.EventId == existing.EventId);
        // The existing person might be inactive now, so make sure they appear in the box even if filtered out.
        var existingPerson = vm.People.FirstOrDefault(p => p.PersonId == existing.PersonId);
        if (existingPerson is not null && !PersonBox.Items.Contains(existingPerson))
            PersonBox.Items.Add(existingPerson);
        PersonBox.SelectedItem = existingPerson;
        TicketBox.Text = existing.TicketType;
        PriceBox.Text = existing.Price.ToString("0.00", CultureInfo.InvariantCulture);
        PaidBox.Text = existing.PaidAmount.ToString("0.00", CultureInfo.InvariantCulture);
        NotesBox.Text = existing.Notes;
        if (existing.Status == PaymentStatus.Free) FreeCheck.IsChecked = true;
        // When editing an existing registration we lock the event and person to avoid
        // breaking the UNIQUE(EventId, PersonId) constraint or losing the historical link.
        EventBox.IsEnabled = false;
        PersonBox.IsEnabled = false;
    }

    private void FreeCheck_Toggled(object sender, RoutedEventArgs e)
    {
        if (FreeCheck.IsChecked == true)
        {
            PriceBox.Text = "0.00";
            PaidBox.Text = "0.00";
            PriceBox.IsEnabled = false;
            PaidBox.IsEnabled = false;
            TicketBox.Text = "Free";
        }
        else
        {
            PriceBox.IsEnabled = true;
            PaidBox.IsEnabled = true;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (EventBox.SelectedItem is not Event selectedEvent)
        {
            MessageBox.Show("Please select an event.", "No event", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (PersonBox.SelectedItem is not Person selectedPerson)
        {
            MessageBox.Show("Please select a person.", "No person", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(PriceBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
        {
            MessageBox.Show("Price must be a number (use a dot as decimal separator).", "Invalid price", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(PaidBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var paid))
        {
            MessageBox.Show("Paid amount must be a number.", "Invalid paid amount", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Registration registration;
            if (_existing is null)
            {
                registration = _vm.RegistrationService.CreateRegistration(
                    selectedEvent, selectedPerson.PersonId, TicketBox.Text.Trim(), price, paid);
                registration.Notes = NotesBox.Text.Trim();
            }
            else
            {
                registration = _existing;
                registration.TicketType = TicketBox.Text.Trim();
                registration.UpdatePayment(price, paid, cancelled: registration.Status == PaymentStatus.Cancelled);
                registration.Notes = NotesBox.Text.Trim();
                var errors = registration.Validate();
                if (errors.Any())
                {
                    MessageBox.Show(string.Join(Environment.NewLine, errors), "Please fix the following", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            Result = registration;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Cannot save", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
