using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using EventPlanner.Domain;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class EventsPage : UserControl
{
    public EventsPage()
    {
        InitializeComponent();
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private Event? SelectedEvent =>
        EventsGrid.SelectedItem is EventRowViewModel row ? row.Event : null;

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Vm is null) return;
        var view = CollectionViewSource.GetDefaultView(Vm.EventRows);
        var text = SearchBox.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(text)) { view.Filter = null; return; }
        view.Filter = obj =>
        {
            if (obj is not EventRowViewModel row) return false;
            return row.Title.Contains(text, StringComparison.OrdinalIgnoreCase)
                || row.Location.Contains(text, StringComparison.OrdinalIgnoreCase)
                || row.Status.ToString().Contains(text, StringComparison.OrdinalIgnoreCase);
        };
    }

    private void EventsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedEvent is not null) OpenDetails(SelectedEvent);
    }

    private void Details_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedEvent is null)
        {
            MessageBox.Show("Please select an event first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        OpenDetails(SelectedEvent);
    }

    private void OpenDetails(Event ev)
    {
        Vm?.OpenEventDetails(ev);
    }

    private void NewEvent_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        var dialog = new EventFormWindow { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try { Vm.EventRepo.Add(dialog.Result); Vm.LoadData(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot save event", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private void EditEvent_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (SelectedEvent is null)
        {
            MessageBox.Show("Please select an event first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new EventFormWindow(SelectedEvent) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try { Vm.EventRepo.Update(dialog.Result); Vm.LoadData(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot update event", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private void DeleteEvent_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null || SelectedEvent is null)
        {
            MessageBox.Show("Please select an event first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var confirm = MessageBox.Show($"Delete '{SelectedEvent.Title}'? Related registrations and expenses will also be removed.",
            "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;
        try { Vm.EventRepo.Delete(SelectedEvent.EventId); Vm.LoadData(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot delete event", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void ArchiveEvent_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null || SelectedEvent is null)
        {
            MessageBox.Show("Please select an event first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        SelectedEvent.Status = EventStatus.Archived;
        try { Vm.EventRepo.Update(SelectedEvent); Vm.LoadData(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot archive event", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
