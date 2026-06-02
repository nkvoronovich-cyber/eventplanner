using System.Windows;
using System.Windows.Controls;
using EventPlanner.Domain;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class RegistrationsPage : UserControl
{
    public RegistrationsPage()
    {
        InitializeComponent();
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private Registration? SelectedRegistration =>
        RegistrationsGrid.SelectedItem is RegistrationRowViewModel row ? row.Registration : null;

    private void NewRegistration_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (!Vm.Events.Any() || !Vm.People.Any(p => p.IsActive))
        {
            MessageBox.Show("Please add at least one event and one active person first.", "Cannot register",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new RegistrationFormWindow(Vm) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try { Vm.RegistrationRepo.AddOrReactivate(dialog.Result); Vm.LoadData(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot save registration", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private void EditRegistration_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (SelectedRegistration is not { } selected)
        {
            MessageBox.Show("Please select a registration first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new RegistrationFormWindow(Vm, selected) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try { Vm.RegistrationRepo.Update(dialog.Result); Vm.LoadData(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot update registration", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private void CancelRegistration_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (SelectedRegistration is not { } selected)
        {
            MessageBox.Show("Please select a registration first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        selected.Cancel();
        try { Vm.RegistrationRepo.Update(selected); Vm.LoadData(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot cancel registration", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void DeleteRegistration_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (SelectedRegistration is not { } selected)
        {
            MessageBox.Show("Please select a registration first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var confirm = MessageBox.Show("Delete this registration?", "Confirm delete",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;
        try { Vm.RegistrationRepo.Delete(selected.RegistrationId); Vm.LoadData(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot delete registration", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
