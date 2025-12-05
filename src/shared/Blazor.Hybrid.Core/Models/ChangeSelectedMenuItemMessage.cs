using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging.Messages;
namespace Blazor.Hybrid.Core;



public sealed class ChangeSelectedMenuItemMessage : ValueChangedMessage<Object>
{
    public ChangeSelectedMenuItemMessage(Object tool)
        : base(tool)
    {
        Guard.IsNotNull(tool);
    }
}
