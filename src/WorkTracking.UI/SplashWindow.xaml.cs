using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace WorkTracking.UI;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion
                      ?? "dev";
        VersionText.Text = $"v{version}";
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Close();
    }

    private void OnClick(object sender, MouseButtonEventArgs e) => Close();
}
