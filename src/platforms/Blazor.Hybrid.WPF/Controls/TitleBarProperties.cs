using System.Windows;

namespace Blazor.Hybrid.Windows.Controls;

/// <summary>
/// Provides attached properties used to adapt the Blazor UI to the window title bar.
/// Mirrors the title-bar dependency properties defined on MicaWindowWithOverlay
/// in the upstream DevToys project, kept here since this fork uses a plain Window.
/// </summary>
public static class TitleBarProperties
{
    /// <summary>
    /// Identifies the TitleBarMarginLeft attached property.
    /// </summary>
    public static readonly DependencyProperty TitleBarMarginLeftProperty =
        DependencyProperty.RegisterAttached(
            "TitleBarMarginLeft",
            typeof(int),
            typeof(TitleBarProperties),
            new PropertyMetadata(0));

    /// <summary>
    /// Identifies the TitleBarMarginRight attached property.
    /// </summary>
    public static readonly DependencyProperty TitleBarMarginRightProperty =
        DependencyProperty.RegisterAttached(
            "TitleBarMarginRight",
            typeof(int),
            typeof(TitleBarProperties),
            new PropertyMetadata(0));

    /// <summary>
    /// Identifies the TitleBarWindowStateButtonsWidth attached property.
    /// </summary>
    public static readonly DependencyProperty TitleBarWindowStateButtonsWidthProperty =
        DependencyProperty.RegisterAttached(
            "TitleBarWindowStateButtonsWidth",
            typeof(int),
            typeof(TitleBarProperties),
            new PropertyMetadata(0));

    public static int GetTitleBarMarginLeft(DependencyObject element)
        => (int)element.GetValue(TitleBarMarginLeftProperty);

    public static void SetTitleBarMarginLeft(DependencyObject element, int value)
        => element.SetValue(TitleBarMarginLeftProperty, value);

    public static int GetTitleBarMarginRight(DependencyObject element)
        => (int)element.GetValue(TitleBarMarginRightProperty);

    public static void SetTitleBarMarginRight(DependencyObject element, int value)
        => element.SetValue(TitleBarMarginRightProperty, value);

    public static int GetTitleBarWindowStateButtonsWidth(DependencyObject element)
        => (int)element.GetValue(TitleBarWindowStateButtonsWidthProperty);

    public static void SetTitleBarWindowStateButtonsWidth(DependencyObject element, int value)
        => element.SetValue(TitleBarWindowStateButtonsWidthProperty, value);
}
