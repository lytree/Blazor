using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Blazor.Hybrid.Avalonia;
public abstract class BaseControl : Control
{

    protected abstract void InternalDispose();

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (e.Root is Window window)
        {
            // need to subscribe the event this way because close gets called after all elements get detached
            window.AddHandler(Window.WindowClosedEvent, (EventHandler<RoutedEventArgs>)OnHostWindowClosed);
        }
        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (e.Root is Window window)
        {
            window.RemoveHandler(Window.WindowClosedEvent, (EventHandler<RoutedEventArgs>)OnHostWindowClosed);
        }
        base.OnDetachedFromLogicalTree(e);
    }

    private void OnHostWindowClosed(object sender, RoutedEventArgs eventArgs)
    {
        ((Window)sender).RemoveHandler(Window.WindowClosedEvent, (EventHandler<RoutedEventArgs>)OnHostWindowClosed);
        InternalDispose();
    }
}
