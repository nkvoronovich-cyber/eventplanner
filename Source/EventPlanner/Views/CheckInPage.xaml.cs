using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EventPlanner.Domain;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class CheckInPage : UserControl
{
    private List<CheckInRow> _all = new();

    public CheckInPage()
    {
        InitializeComponent();
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private void CheckInPage_Loaded(object sender, RoutedEventArgs e) => RebuildList();

    private void RebuildList()
    {
        if (Vm is null) return;
        _all = Vm.Registrations
            .Where(r => r.Status != PaymentStatus.Cancelled)
            .Select(r =>
            {
                var person = Vm.People.FirstOrDefault(p => p.PersonId == r.PersonId);
                var ev = Vm.Events.FirstOrDefault(x => x.EventId == r.EventId);
                return new CheckInRow(r, person, ev);
            })
            .OrderBy(row => row.PersonName)
            .ToList();
        ParticipantBox.ItemsSource = _all;
        ApplyFilter();
    }

    private void FilterBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        var view = CollectionViewSource.GetDefaultView(_all);
        if (view is null) return;
        var text = FilterBox.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(text)) { view.Filter = null; return; }
        view.Filter = obj =>
        {
            if (obj is not CheckInRow row) return false;
            return row.PersonName.Contains(text, StringComparison.OrdinalIgnoreCase)
                || row.PersonPhone.Contains(text, StringComparison.OrdinalIgnoreCase);
        };
    }

    private void ParticipantBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ParticipantBox.SelectedItem is not CheckInRow row)
        {
            DetailsPanel.Visibility = Visibility.Collapsed;
            StatusText.Text = "Pick a participant from the list above to see their details.";
            return;
        }
        StatusText.Text = "Participant selected.";
        NameText.Text = row.PersonName;
        PhoneText.Text = string.IsNullOrEmpty(row.PersonPhone) ? "No phone on file" : $"Phone: {row.PersonPhone}";
        EventText.Text = $"Event: {row.EventTitle}";
        PaymentText.Text = $"Payment: {row.Registration.Status} ({row.Registration.PaidAmount:0.00} € of {row.Registration.Price:0.00} €)";
        CheckedText.Text = row.Registration.CheckedInAt is null
            ? "Not yet checked in."
            : $"Already checked in at {row.Registration.CheckedInAt:dd.MM.yyyy HH:mm}";
        DetailsPanel.Visibility = Visibility.Visible;
    }

    private void CheckIn_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null || ParticipantBox.SelectedItem is not CheckInRow row)
        {
            MessageBox.Show("Please pick a participant first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            row.Registration.CheckIn();
            Vm.RegistrationRepo.Update(row.Registration);
            Vm.LoadData();
            RebuildList();
            CheckedText.Text = $"Checked in at {row.Registration.CheckedInAt:dd.MM.yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Cannot check in", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        FilterBox.Clear();
        ParticipantBox.SelectedIndex = -1;
        DetailsPanel.Visibility = Visibility.Collapsed;
        StatusText.Text = "Pick a participant from the list above to see their details.";
    }
}

public class CheckInRow
{
    public Registration Registration { get; }
    public string PersonName { get; }
    public string PersonPhone { get; }
    public string EventTitle { get; }
    public string DisplayLabel { get; }

    public CheckInRow(Registration r, Person? person, Event? ev)
    {
        Registration = r;
        PersonName = person?.FullName ?? "(unknown person)";
        PersonPhone = person?.Phone ?? "";
        EventTitle = ev?.Title ?? "(unknown event)";
        var checkedMark = r.CheckedInAt is null ? "" : " ✓";
        DisplayLabel = $"{PersonName} — {EventTitle}{checkedMark}";
    }
}
