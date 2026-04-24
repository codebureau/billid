using WorkTracking.UI.ViewModels;

namespace WorkTracking.UI.Views;

public partial class AddClientDialog : System.Windows.Window
{
    public AddClientDialog(AddClientViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.CloseRequested += (_, _) => Close();
    }
}
