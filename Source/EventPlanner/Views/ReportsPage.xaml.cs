using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using EventPlanner.Domain;
using EventPlanner.Services;
using EventPlanner.ViewModels;

namespace EventPlanner.Views;

public partial class ReportsPage : UserControl
{
    public ReportsPage()
    {
        InitializeComponent();
        Loaded += ReportsPage_Loaded;
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private void ReportsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        EventBox.Items.Clear();
        foreach (var ev in Vm.Events) EventBox.Items.Add(ev);
        if (EventBox.Items.Count > 0) EventBox.SelectedIndex = 0;
    }

    private void NewEvent_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        var dialog = new EventFormWindow { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
        {
            try
            {
                Vm.EventRepo.Add(dialog.Result);
                Vm.LoadData();
                EventBox.Items.Clear();
                foreach (var ev in Vm.Events) EventBox.Items.Add(ev);
                EventBox.SelectedItem = Vm.Events.LastOrDefault();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Cannot save event", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }
    }

    private string? AskForCsvPath(string suggestedName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            FileName = suggestedName,
            DefaultExt = ".csv"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void ShowPreview(string path, int rowCount)
    {
        var info = new FileInfo(path);
        PreviewText.Text = $"Saved {rowCount} row(s) to:{Environment.NewLine}{path}{Environment.NewLine}{Environment.NewLine}" +
                           $"File size: {info.Length} bytes";
    }

    private void ExportAttendees_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        var path = AskForCsvPath("attendees.csv");
        if (path is null) return;
        try
        {
            var rows = Vm.Registrations.ToList();
            CsvExportService.ExportRegistrations(path, rows);
            ShowPreview(path, rows.Count);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ExportUnpaid_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        var path = AskForCsvPath("unpaid.csv");
        if (path is null) return;
        try
        {
            var rows = Vm.Registrations
                .Where(r => r.Status == PaymentStatus.Unpaid || r.Status == PaymentStatus.PartPaid)
                .ToList();
            CsvExportService.ExportUnpaid(path, Vm.Registrations);
            ShowPreview(path, rows.Count);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ExportFinance_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (EventBox.SelectedItem is not Event selected)
        {
            MessageBox.Show("Please choose an event first.", "No event", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var safeName = string.Join("_", selected.Title.Split(Path.GetInvalidFileNameChars()));
        var path = AskForCsvPath($"finance_{safeName}.csv");
        if (path is null) return;
        try
        {
            var regs = Vm.Registrations.Where(r => r.EventId == selected.EventId).ToList();
            var exps = Vm.Expenses.Where(x => x.EventId == selected.EventId).ToList();
            CsvExportService.ExportFinanceSummary(path, selected, regs, exps);
            ShowPreview(path, 6);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
