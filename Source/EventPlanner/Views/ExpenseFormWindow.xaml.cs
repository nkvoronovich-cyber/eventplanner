using System.Globalization;
using System.Windows;
using EventPlanner.Domain;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class ExpenseFormWindow : Window
{
    public Expense? Result { get; private set; }
    private readonly Expense _working;

    public ExpenseFormWindow(MainViewModel vm)
    {
        InitializeComponent();
        foreach (var ev in vm.Events) EventBox.Items.Add(ev);
        if (EventBox.Items.Count > 0) EventBox.SelectedIndex = 0;
        _working = new Expense { CreatedAt = DateTime.Now };
    }

    public ExpenseFormWindow(MainViewModel vm, Expense existing) : this(vm)
    {
        _working = new Expense
        {
            ExpenseId = existing.ExpenseId,
            EventId = existing.EventId,
            Category = existing.Category,
            Amount = existing.Amount,
            Notes = existing.Notes,
            PaidAt = existing.PaidAt,
            CreatedAt = existing.CreatedAt
        };
        EventBox.SelectedItem = vm.Events.FirstOrDefault(e => e.EventId == existing.EventId);
        CategoryBox.Text = existing.Category;
        AmountBox.Text = existing.Amount.ToString("0.00", CultureInfo.InvariantCulture);
        PaidDate.SelectedDate = existing.PaidAt;
        NotesBox.Text = existing.Notes;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (EventBox.SelectedItem is not Event selectedEvent)
        {
            MessageBox.Show("Please select an event.", "No event", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(AmountBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            MessageBox.Show("Amount must be a number.", "Invalid amount", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _working.EventId = selectedEvent.EventId;
        _working.Category = CategoryBox.Text.Trim();
        _working.Amount = amount;
        _working.Notes = NotesBox.Text.Trim();
        _working.PaidAt = PaidDate.SelectedDate;

        var errors = _working.Validate();
        if (errors.Any())
        {
            MessageBox.Show(string.Join(Environment.NewLine, errors), "Please fix the following", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Result = _working;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
