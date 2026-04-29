using Serilog;
using System.Windows;
using Velopack;

namespace WorkTracking.UI;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        try
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal unhandled exception in Main");
            Log.CloseAndFlush();
            MessageBox.Show(
                $"billid failed to start.\n\n{ex.Message}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
