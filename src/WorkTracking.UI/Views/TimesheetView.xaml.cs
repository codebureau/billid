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

    private void OnRowDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_vm?.EditOrViewEntryCommand.CanExecute(null) == true)
            _vm.EditOrViewEntryCommand.Execute(null);
    }

    private void OnAttachmentsPaneDragOver(object sender, DragEventArgs e)
    {
        e.Effects = _vm?.CanAddAttachment == true && e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnAttachmentsPaneDrop(object sender, DragEventArgs e)
    {
        if (_vm is null || !_vm.CanAddAttachment) return;
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (var file in files)
            await _vm.AttachFileAsync(file);
    }
}
