using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace WorkTracking.UI;

public partial class SplashWindow : Window
{
    private readonly bool _canClose;

    public SplashWindow(bool canClose = false)
    {
        InitializeComponent();
        _canClose = canClose;
        CloseButton.Visibility = canClose ? Visibility.Visible : Visibility.Collapsed;

        var version = Assembly.GetExecutingAssembly()
                          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                          ?.InformationalVersion
                          ?.Split('+')[0]
                      ?? "dev";
        VersionText.Text = $"v{version}";
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        // In splash mode (not About), any click closes the window
        if (!_canClose)
            Close();
    }

    private void OnBorderClick(object sender, MouseButtonEventArgs e) { /* handled by OnMouseLeftButtonDown */ }

    private void OnClick(object sender, RoutedEventArgs e) => Close();
}
