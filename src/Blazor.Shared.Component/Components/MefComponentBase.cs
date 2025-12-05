using System.Diagnostics.CodeAnalysis;
using Blazor.Hybrid.Core;
using Microsoft.AspNetCore.Components;

namespace Blazor.Shared.Components;

public abstract class MefComponentBase : JSStyledComponentBase
{

    [Inject]
    protected IMefProvider MefProvider { get; set; } = default!;

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();
    }
}
