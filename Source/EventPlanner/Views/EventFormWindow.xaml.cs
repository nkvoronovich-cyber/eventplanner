using System.Globalization;
using System.Windows;
using EventPlanner.Domain;

namespace EventPlanner.Views;

public partial class EventFormWindow : Window
{
    public Event? Result { get; private set; }
    private readonly Event _working;

    public EventFormWindow() : this(new Event { StartAt = DateTime.Now.AddDays(7), Capacity = 50, Status = EventStatus.Draft }) { }

    public EventFormWindow(Event existing)
    {
        InitializeComponent();
        _working = new Event
        {
            EventId = existing.EventId,
            Title = existing.Title,
            StartAt = existing.StartAt,
            Location = existing.Location,
            Capacity = existing.Capacity,
            Status = existing.Status,
            Description = existing.Description
        };
        foreach (var status in Enum.GetValues<EventStatus>()) StatusBox.Items.Add(status);
        BindToUi();
    }

    private void BindToUi()
    {
        TitleBox.Text = _working.Title;
        DatePick.SelectedDate = _working.StartAt.Date;
        TimeBox.Text = _working.StartAt.ToString("HH:mm", CultureInfo.InvariantCulture);
        LocationBox.Text = _working.Location;
        CapacityBox.Text = _working.Capacity.ToString(CultureInfo.InvariantCulture);
        StatusBox.SelectedItem = _working.Status;
        DescriptionBox.Text = _working.Description;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var date = DatePick.SelectedDate ?? DateTime.Today;
        if (!TimeSpan.TryParse(TimeBox.Text, CultureInfo.InvariantCulture, out var time))
        {
            MessageBox.Show("Time must be in HH:mm format.", "Invalid time", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!int.TryParse(CapacityBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var capacity))
        {
            MessageBox.Show("Capacity must be a whole number.", "Invalid capacity", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _working.Title = TitleBox.Text.Trim();
        _working.StartAt = date.Add(time);
        _working.Location = LocationBox.Text.Trim();
        _working.Capacity = capacity;
        _working.Status = StatusBox.SelectedItem is EventStatus s ? s : EventStatus.Draft;
        _working.Description = DescriptionBox.Text.Trim();

        var errors = _working.Validate();
        if (errors.Any())
        {
            MessageBox.Show(string.Join(Environment.NewLine, errors), "Please fix the following", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Result = _working;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
