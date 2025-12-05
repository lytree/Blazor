using Blazor.Hybrid.Core;
using Blazor.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Blazor.Shared.Components;

public abstract class StyledComponentBase : ComponentBase
{
    private static readonly Random random = new();

    private readonly Lazy<ObservableHashSet<string>> _css;

    protected readonly HashSet<StyledComponentBase> ChildrenComponents = new();

    /// <summary>
    /// Gets a reference to the HTML element rendered by the component.
    /// </summary>
    public ElementReference Element { get; set; }

    /// <summary>
    /// Gets the value of the <c>id</c> attribute in the DOM of this component.
    /// </summary>
    public string Id { get; protected set; } = NewId();

    /// <summary>
    /// Gets or sets additional custom attributes that will be rendered by the component.
    /// </summary>
    /// <value>The attributes.</value>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets a custom CSS class to apply to the component.
    /// </summary>
    [Parameter]
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a custom CSS style to apply to the component.
    /// </summary>
    [Parameter]
    public string Style { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the component is enabled.
    /// </summary>
    [Parameter]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the parent component is enabled or not.
    /// </summary>
    [CascadingParameter(Name = nameof(ParentIsEnabled))]
    protected bool ParentIsEnabled { get; set; } = true;

    /// <summary>
    /// Gets whether the component is actually enabled or not, based on <see cref="IsEnabled"/> and <see cref="ParentIsEnabled"/>.
    /// </summary>
    public bool IsActuallyEnabled => IsEnabled && ParentIsEnabled;

    /// <summary>
    /// Gets or sets how the component should horizontally align.
    /// </summary>
    [Parameter]
    public UIHorizontalAlignment HorizontalAlignment { get; set; } = UIHorizontalAlignment.Stretch;

    /// <summary>
    /// Gets or sets how the component should vertically align.
    /// </summary>
    [Parameter]
    public UIVerticalAlignment VerticalAlignment { get; set; } = UIVerticalAlignment.Stretch;

    /// <summary>
    /// Gets or sets the width the component should have.
    /// If null, the component will take as much space as it can.
    /// </summary>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height the component should have.
    /// If null, the component will take as much space as it can.
    /// </summary>
    [Parameter]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets whether the component should be visible. Default is true.
    /// </summary>
    [Parameter]
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the outer-space to set on the left of the component.
    /// If null, the component will use the default margin.
    /// </summary>
    [Parameter]
    public int? MarginLeft { get; set; }

    /// <summary>
    /// Gets or sets the outer-space to set on the right of the component.
    /// If null, the component will use the default margin.
    /// </summary>
    [Parameter]
    public int? MarginRight { get; set; }

    /// <summary>
    /// Gets or sets the outer-space to set on the top of the component.
    /// If null, the component will use the default margin.
    /// </summary>
    [Parameter]
    public int? MarginTop { get; set; }

    /// <summary>
    /// Gets or sets the outer-space to set on the bottom of the component.
    /// If null, the component will use the default margin.
    /// </summary>
    [Parameter]
    public int? MarginBottom { get; set; }

    /// <summary>
    /// Gets or sets the inner-space to set on the left of the component.
    /// If null, the component will use the default margin.
    /// </summary>
    [Parameter]
    public int? PaddingLeft { get; set; }

    /// <summary>
    /// Gets or sets the inner-space to set on the right of the component.
    /// If null, the component will use the default margin.
    /// </summary>
    [Parameter]
    public int? PaddingRight { get; set; }

    /// <summary>
    /// Gets or sets the inner-space to set on the top of the component.
    /// If null, the component will use the default margin.
    /// </summary>
    [Parameter]
    public int? PaddingTop { get; set; }

    /// <summary>
    /// Gets or sets the inner-space to set on the bottom of the component.
    /// If null, the component will use the default margin.
    /// </summary>
    [Parameter]
    public int? PaddingBottom { get; set; }

    [Parameter]
    public StyledComponentBase? ParentComponent { get; set; }

    /// <summary>
    /// Gets a list of CSS class to apply to the component.
    /// </summary>
    protected ObservableHashSet<string> CSS => _css.Value;

    /// <summary>
    /// Gets an aggregation of <see cref="CSS"/>, <see cref="Class"/> and (optionally) <c>class</c> attribute defined in <see cref="AdditionalAttributes"/>.
    /// </summary>
    public string FinalCssClasses { get; private set; } = string.Empty;

    protected StyledComponentBase()
    {
        _css = new(() =>
        {
            var collection = new ObservableHashSet<string>();
            collection.CollectionChanged += CSS_CollectionChanged;
            return collection;
        });
    }

    /// <inheritdoc />
    public new void StateHasChanged()
    {
        InvokeAsync(() =>
        {
            base.StateHasChanged();
        });
    }

    private void CSS_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        BuildClassAttribute();
    }

    private void BuildClassAttribute()
    {
        var cssBuilder = new CssBuilder();
        if (_css.IsValueCreated)
        {
            foreach (string cssClass in _css.Value)
            {
                cssBuilder.AddClass(cssClass);
            }
        }

        if (AdditionalAttributes is not null)
        {
            cssBuilder.AddClassFromAttributes(AdditionalAttributes);
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            cssBuilder.AddClass(Class);
        }

        FinalCssClasses = cssBuilder.ToString();
    }

    protected override void OnParametersSet()
    {
        BuildClassAttribute();

        base.OnParametersSet();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ParentComponent?.RegisterChildComponent(this);
    }

    protected void RegisterChildComponent(StyledComponentBase component)
    {
        Guard.IsNotNull(component);
        ChildrenComponents.Add(component);
    }

    /// <summary>
    /// Returns a new small Id.
    /// HTML id must start with a letter.
    /// Example: f127d9edf14385adb
    /// </summary>
    /// <returns></returns>
    protected static string NewId(int length = 8)
    {
        Guard.IsLessThanOrEqualTo(length, 16);

        if (length <= 8)
        {
            return $"f{random.Next():x}";
        }

        return $"f{random.Next():x}{random.Next():x}"[..length];
    }
}


/// <summary>
/// Defines the different orientations that an element or layout can have.
/// </summary>
[Flags]
public enum UIOrientation
{
    /// <summary>
    /// Element or layout should be horizontally oriented.
    /// </summary>
    Horizontal = 1,

    /// <summary>
    /// Element or layout should be vertically oriented.
    /// </summary>
    Vertical = 2
}


/// <summary>
/// Defines the different spacing to apply between element in the layout.
/// </summary>
public enum UISpacing
{
    /// <summary>
    /// No space between element. Use this space between element or text that should be closed from each other.
    /// </summary>
    None = 0,

    /// <summary>
    /// A small space between element. Use this space between elements of a same group.
    /// </summary>
    Small = 1,

    /// <summary>
    /// A medium space between element. Use this space to separate groups of elements.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// A large space between element. Use this space to separate sections.
    /// </summary>
    Large = 3
}

/// <summary>
/// Indicates where an element should be displayed on the horizontal axis relative to the allocated layout slot of the parent element.
/// </summary>
public enum UIHorizontalAlignment
{
    /// <summary>
    /// An element aligned to the left of the layout slot for the parent element.
    /// </summary>
    Left,

    /// <summary>
    /// An element aligned to the center of the layout slot for the parent element.
    /// </summary>
    Center,

    /// <summary>
    /// An element aligned to the right of the layout slot for the parent element.
    /// </summary>
    Right,

    /// <summary>
    /// An element stretched to fill the entire layout slot of the parent element.
    /// </summary>
    Stretch,
}

/// <summary>
/// Describes how a child element is vertically positioned or stretched within a parent's layout slot.
/// </summary>
public enum UIVerticalAlignment
{
    /// <summary>
    /// The child element is aligned to the top of the parent's layout slot.
    /// </summary>
    Top,

    /// <summary>
    /// The child element is aligned to the center of the parent's layout slot.
    /// </summary>
    Center,

    /// <summary>
    /// The child element is aligned to the bottom of the parent's layout slot.
    /// </summary>
    Bottom,

    /// <summary>
    /// The child element stretches to fill the parent's layout slot.
    /// </summary>
    Stretch
}
