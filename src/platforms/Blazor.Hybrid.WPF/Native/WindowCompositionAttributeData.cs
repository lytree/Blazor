using System.Runtime.InteropServices;

namespace Blazor.Hybrid.Windows.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct WindowCompositionAttributeData
{
    public WindowCompositionAttribute Attribute;
    public IntPtr Data;
    public int SizeOfData;
}
