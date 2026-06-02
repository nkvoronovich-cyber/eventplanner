using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EventPlanner.Domain;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class PeoplePage : UserControl
{
    public PeoplePage()
    {
        InitializeComponent();
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Vm is null) return;
        var view = CollectionViewSource.GetDefaultView(Vm.People);
        var text = SearchBox.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(text)) { view.Filter = null; return; }
        view.Filter = obj =>
        {
            if (obj is not Person p) return false;
            return p.FullName.Contains(text, StringComparison.OrdinalIgnoreCase)
                || p.Phone.Contains(text, StringComparison.OrdinalIgnoreCase)
                || p.Email.Contains(text, StringComparison.OrdinalIgnoreCase);
        };
    }

    private void PeopleGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Show "Reactivate" for inactive people, "Deactivate" for active ones.
        if (PeopleGrid.SelectedItem is Person selected)
            ToggleActiveButton.Content = selected.IsActive ? "Deactivate" : "Reactivate";
        else
            ToggleActiveButton.Content = "Deactivate";
    }

    private void AddPerson_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        var dialog = new PersonFormWindow { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try { Vm.PersonRepo.Add(dialog.Result); Vm.LoadData(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot save person", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private void EditPerson_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (PeopleGrid.SelectedItem is not Person selected)
        {
            MessageBox.Show("Please select a person first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new PersonFormWindow(selected) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try { Vm.PersonRepo.Update(dialog.Result); Vm.LoadData(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot update person", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private void ToggleActive_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (PeopleGrid.SelectedItem is not Person selected)
        {
            MessageBox.Show("Please select a person first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        selected.IsActive = !selected.IsActive;
        try { Vm.PersonRepo.Update(selected); Vm.LoadData(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot change status", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void DeletePerson_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (PeopleGrid.SelectedItem is not Person selected)
        {
            MessageBox.Show("Please select a person first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var confirm = MessageBox.Show($"Delete '{selected.FullName}'? Related registrations will also be removed.",
            "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;
        try { Vm.PersonRepo.Delete(selected.PersonId); Vm.LoadData(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot delete person", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
