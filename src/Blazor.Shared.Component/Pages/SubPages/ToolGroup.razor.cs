
using Blazor.Shared.Components;
using Blazor.Shared.Core;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace Blazor.Shared.Pages.SubPages;

public partial class ToolGroup : MefComponentBase, IFocusable
{
    internal static readonly Lazy<string> DisplayVersionNumber = new(() =>
    {
        var assemblyInformationalVersion
            = (AssemblyInformationalVersionAttribute)
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))!;
        return "v" + assemblyInformationalVersion.InformationalVersion;
    });

    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Pages/SubPages/ToolGroup.razor.js";

    protected string HeroId => Id + "-hero";

    [Import]
    internal ToolGroupPageViewModel ViewModel { get; set; } = default!;


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && ViewModel.IsAllToolsGroup)
        {
            using (await Semaphore.WaitAsync(CancellationToken.None))
            {
                Guard.IsNotNull(_gridView);
                await (await JSModule).InvokeVoidWithErrorHandlingAsync("initializeHeroParallax", _gridView.ScrollViewerId, HeroId);
            }
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (GroupViewItem is not null)
        {
            ViewModel.Load(GroupViewItem);
        }
    }

    public ValueTask<bool> FocusAsync()
    {
        Guard.IsNotNull(_gridView);
        return _gridView.FocusAsync();
    }

    private void OnToolSelected(object item)
    {
        Guard.IsOfType<GuiToolInstance>(item);
        ViewModel.ToolSelectedCommand.Execute((GuiToolInstance)item);
    }

    private void OnOpenInNewWindow(GuiToolInstance item)
    {
        CommandLineLauncherService.LaunchTool(item.InternalComponentName);
    }

#pragma warning disable CA1822 // Mark members as static
    private void OnSuggestToolIdeaClick()
    {
        // OSHelper.OpenFileInShell("https://github.com/veler/DevToys/issues/new/choose");
    }
#pragma warning restore CA1822 // Mark members as static
}
