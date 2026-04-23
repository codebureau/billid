using WorkTracking.UI.ViewModels;

namespace WorkTracking.UI.Views;

public partial class WorkEntryDialog : System.Windows.Window
{
    public WorkEntryDialog(WorkEntryDialogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.CloseRequested += (_, _) => Close();
    }
}
