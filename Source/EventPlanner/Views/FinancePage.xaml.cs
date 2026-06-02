using System.Windows;
using System.Windows.Controls;
using EventPlanner.Domain;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class FinancePage : UserControl
{
    public FinancePage()
    {
        InitializeComponent();
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private void AddExpense_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (!Vm.Events.Any())
        {
            MessageBox.Show("Please add at least one event first.", "Cannot add expense", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new ExpenseFormWindow(Vm) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try { Vm.ExpenseRepo.Add(dialog.Result); Vm.LoadData(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot save expense", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private void EditExpense_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (ExpensesGrid.SelectedItem is not Expense selected)
        {
            MessageBox.Show("Please select an expense first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dialog = new ExpenseFormWindow(Vm, selected) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try { Vm.ExpenseRepo.Update(dialog.Result); Vm.LoadData(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot update expense", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private void DeleteExpense_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (ExpensesGrid.SelectedItem is not Expense selected)
        {
            MessageBox.Show("Please select an expense first.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var confirm = MessageBox.Show("Delete this expense?", "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;
        try { Vm.ExpenseRepo.Delete(selected.ExpenseId); Vm.LoadData(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot delete expense", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
