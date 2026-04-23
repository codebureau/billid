namespace WorkTracking.UI.Services;

public interface IDialogService
{
    void ShowError(string message, string title = "Error");
    bool Confirm(string message, string title = "Confirm");
    string? PickOpenFile(string filter = "All files|*.*");
    string? PickSaveFile(string filter = "All files|*.*", string defaultFileName = "");
    bool ShowInvoicePrepDialog(ViewModels.InvoicePrepViewModel vm);
}
