using System.Windows;
using Microsoft.Win32;

namespace WorkTracking.UI.Services;

public class DialogService : IDialogService
{
    public void ShowError(string message, string title = "Error")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool Confirm(string message, string title = "Confirm")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public string? PickOpenFile(string filter = "All files|*.*")
    {
        var dialog = new OpenFileDialog { Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PickSaveFile(string filter = "All files|*.*", string defaultFileName = "")
    {
        var dialog = new SaveFileDialog { Filter = filter, FileName = defaultFileName };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public bool ShowInvoicePrepDialog(ViewModels.InvoicePrepViewModel vm)
    {
        var dialog = new Views.InvoicePrepDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
        return vm.Confirmed;
    }

    public bool ShowAddClientDialog(ViewModels.AddClientViewModel vm)
    {
        var dialog = new Views.AddClientDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
        return vm.Confirmed;
    }

    public bool ShowWorkEntryDialog(ViewModels.WorkEntryDialogViewModel vm)
    {
        var dialog = new Views.WorkEntryDialog(vm)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
        return vm.Confirmed;
    }
}
