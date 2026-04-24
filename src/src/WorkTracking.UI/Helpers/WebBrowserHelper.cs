using System.Windows;
using System.Windows.Controls;

namespace WorkTracking.UI.Converters;

public static class WebBrowserHelper
{
    public static readonly DependencyProperty HtmlSourceProperty =
        DependencyProperty.RegisterAttached(
            "HtmlSource",
            typeof(string),
            typeof(WebBrowserHelper),
            new PropertyMetadata(null, OnHtmlSourceChanged));

    public static string? GetHtmlSource(DependencyObject obj) => (string?)obj.GetValue(HtmlSourceProperty);
    public static void SetHtmlSource(DependencyObject obj, string value) => obj.SetValue(HtmlSourceProperty, value);

    private static void OnHtmlSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WebBrowser browser)
            browser.NavigateToString((string?)e.NewValue ?? "<html><body></body></html>");
    }
}
