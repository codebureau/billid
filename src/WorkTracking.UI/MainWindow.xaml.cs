using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WorkTracking.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/app.ico", UriKind.Absolute));
    }
}
