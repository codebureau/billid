using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WorkTracking.UI.Behaviors;

/// <summary>
/// Attached behavior that enables drag-drop reordering on a ListBox.
/// Bind ReorderCommand to an ICommand that accepts (int fromIndex, int toIndex).
/// </summary>
public static class ListBoxDragDropReorderBehavior
{
    // --- Attached properties ---

    public static readonly DependencyProperty ReorderCommandProperty =
        DependencyProperty.RegisterAttached(
            "ReorderCommand",
            typeof(ICommand),
            typeof(ListBoxDragDropReorderBehavior),
            new PropertyMetadata(null, OnReorderCommandChanged));

    public static ICommand? GetReorderCommand(DependencyObject obj) =>
        (ICommand?)obj.GetValue(ReorderCommandProperty);

    public static void SetReorderCommand(DependencyObject obj, ICommand? value) =>
        obj.SetValue(ReorderCommandProperty, value);

    // --- Drag state ---

    private static ListBoxItem? _dragSource;
    private static Point _dragStartPoint;
    private static bool _isDragging;
    private const double DragThreshold = 4;

    // --- Hook / unhook ---

    private static void OnReorderCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox) return;

        if (e.OldValue is not null)
        {
            listBox.PreviewMouseLeftButtonDown -= OnMouseDown;
            listBox.PreviewMouseMove            -= OnMouseMove;
            listBox.Drop                        -= OnDrop;
            listBox.DragOver                    -= OnDragOver;
        }

        if (e.NewValue is not null)
        {
            listBox.AllowDrop = true;
            listBox.PreviewMouseLeftButtonDown += OnMouseDown;
            listBox.PreviewMouseMove            += OnMouseMove;
            listBox.Drop                        += OnDrop;
            listBox.DragOver                    += OnDragOver;
        }
    }

    // --- Event handlers ---

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        _dragSource = null;

        if (sender is not ListBox lb) return;
        var item = GetListBoxItemFromPoint(lb, e.GetPosition(lb));
        if (item is null) return;

        _dragSource = item;
        _dragStartPoint = e.GetPosition(lb);
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragSource is null || e.LeftButton != MouseButtonState.Pressed) return;
        if (sender is not ListBox lb) return;

        var pos = e.GetPosition(lb);
        if (!_isDragging)
        {
            var delta = pos - _dragStartPoint;
            if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold) return;
            _isDragging = true;
        }

        DragDrop.DoDragDrop(_dragSource, _dragSource.DataContext!, DragDropEffects.Move);
        _isDragging = false;
        _dragSource = null;
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not ListBox lb) return;

        var command = GetReorderCommand(lb);
        if (command is null) return;

        var targetItem = GetListBoxItemFromPoint(lb, e.GetPosition(lb));
        if (targetItem is null) return;

        var draggedData = e.Data.GetData(e.Data.GetFormats()[0]);
        if (draggedData is null) return;

        var items = lb.ItemsSource?.Cast<object>().ToList();
        if (items is null) return;

        var fromIndex = items.IndexOf(draggedData);
        var toIndex   = items.IndexOf(targetItem.DataContext!);

        if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex) return;

        if (command.CanExecute((fromIndex, toIndex)))
            command.Execute((fromIndex, toIndex));

        e.Handled = true;
    }

    // --- Helper ---

    private static ListBoxItem? GetListBoxItemFromPoint(ListBox lb, Point point)
    {
        var element = lb.InputHitTest(point) as DependencyObject;
        while (element is not null)
        {
            if (element is ListBoxItem item) return item;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }
}
