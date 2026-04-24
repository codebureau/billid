using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.UI.Views;

public partial class TimesheetView : UserControl
{
    public TimesheetView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private TimesheetViewModel? _vm;

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
            _vm.PropertyChanged -= OnVmPropertyChanged;

        _vm = e.NewValue as TimesheetViewModel;

        if (_vm is not null)
            _vm.PropertyChanged += OnVmPropertyChanged;

        NavigateNotes();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimesheetViewModel.RenderedNotesHtml))
            NavigateNotes();
    }

    private void NavigateNotes()
    {
        var html = _vm?.RenderedNotesHtml ?? "<html><body></body></html>";
        NotesWebBrowser.NavigateToString(html);
    }
}
