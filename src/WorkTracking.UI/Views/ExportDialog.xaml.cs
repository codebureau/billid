namespace WorkTracking.UI.Views;

public partial class ExportDialog : System.Windows.Window
{
    public ExportDialog(ViewModels.ExportViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.CloseRequested += (_, confirmed) =>
        {
            DialogResult = confirmed;
            Close();
        };
    }
}
