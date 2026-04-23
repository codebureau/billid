using Microsoft.Win32;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.UI.Views;

public partial class InvoicePrepDialog : System.Windows.Window
{
    public InvoicePrepDialog(InvoicePrepViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.CloseRequested += (_, _) => Close();
    }

    private void BrowsePdf_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "PDF files|*.pdf|All files|*.*" };
        if (dialog.ShowDialog() == true)
            ((InvoicePrepViewModel)DataContext).PdfPath = dialog.FileName;
    }
}
