using System;
using System.Windows;
using System.Windows.Media.Imaging;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/app-icon.png", UriKind.Absolute));
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainWindowViewModel oldVm)
            oldVm.ShowAboutRequested -= OnShowAboutRequested;
        if (e.NewValue is MainWindowViewModel newVm)
            newVm.ShowAboutRequested += OnShowAboutRequested;
    }

    private void OnShowAboutRequested(object? sender, EventArgs e)
    {
        var splash = new SplashWindow(canClose: true);
        splash.Owner = this;
        splash.ShowDialog();
    }
}
