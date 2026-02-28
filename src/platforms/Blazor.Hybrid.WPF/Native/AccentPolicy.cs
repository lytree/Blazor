using System.Runtime.InteropServices;

namespace Blazor.Hybrid.Windows.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct AccentPolicy
{
    public AccentState AccentState;
    public AccentFlag AccentFlags;
    public uint GradientColor;
    public int AnimationId;
}
