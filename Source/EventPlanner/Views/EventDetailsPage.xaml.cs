using System.Windows;
using System.Windows.Controls;
using EventPlanner.Domain;
using EventPlanner.Services;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class EventDetailsPage : UserControl
{
    public EventDetailsPage()
    {
        InitializeComponent();
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private void EventDetailsPage_Loaded(object sender, RoutedEventArgs e) => Refresh();

    private void Refresh()
    {
        if (Vm?.SelectedEvent is not { } ev) return;

        TitleText.Text = ev.Title;
        DateText.Text = ev.StartAt.ToString("dd.MM.yyyy HH:mm");
        LocationText.Text = ev.Location;
        CapacityText.Text = ev.Capacity.ToString();
        StatusText.Text = ev.Status.ToString();
        DescriptionText.Text = string.IsNullOrWhiteSpace(ev.Description) ? "(no description)" : ev.Description;

        var registrations = Vm.Registrations.Where(r => r.EventId == ev.EventId).ToList();
        var expenses = Vm.Expenses.Where(x => x.EventId == ev.EventId).ToList();

        var registrationRows = registrations.Select(r => new RegistrationRow(r,
            Vm.People.FirstOrDefault(p => p.PersonId == r.PersonId)?.FullName ?? "(unknown)")).ToList();
        RegistrationsGrid.ItemsSource = registrationRows;

        var registeredPersonIds = registrations.Where(r => r.Status != PaymentStatus.Cancelled)
            .Select(r => r.PersonId).ToHashSet();
        var speakers = Vm.People.OfType<Speaker>().Where(s => registeredPersonIds.Contains(s.PersonId)).ToList();
        SpeakersGrid.ItemsSource = speakers;

        ExpectedCard.Text = $"{FinanceService.ExpectedIncome(registrations):0.00} €";
        PaidCard.Text = $"{FinanceService.PaidIncome(registrations):0.00} €";
        OutstandingCard.Text = $"{FinanceService.Outstanding(registrations):0.00} €";
        ExpensesCard.Text = $"{FinanceService.TotalExpenses(expenses):0.00} €";
        ProfitCard.Text = $"{FinanceService.Profit(registrations, expenses):0.00} €";

        ExpensesGrid.ItemsSource = expenses;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        Vm.CurrentPage = AppPage.Events;
    }

    private void AddRegistration_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null || Vm.SelectedEvent is null) return;
        if (!Vm.People.Any(p => p.IsActive))
        {
            MessageBox.Show("Please add at least one active person first.", "Cannot register",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new RegistrationFormWindow(Vm, Vm.SelectedEvent) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try
            {
                Vm.RegistrationRepo.AddOrReactivate(dialog.Result);
                Vm.LoadData();
                Refresh();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot save registration", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }
}

public class RegistrationRow
{
    public int RegistrationId { get; }
    public string PersonName { get; }
    public string TicketType { get; }
    public decimal Price { get; }
    public decimal PaidAmount { get; }
    public PaymentStatus Status { get; }
    public DateTime? CheckedInAt { get; }

    public RegistrationRow(Registration r, string personName)
    {
        RegistrationId = r.RegistrationId;
        PersonName = personName;
        TicketType = r.TicketType;
        Price = r.Price;
        PaidAmount = r.PaidAmount;
        Status = r.Status;
        CheckedInAt = r.CheckedInAt;
    }
}
