using System.Reflection;
using System.Runtime.Loader;

namespace Blazor.Hybrid.Avalonia;

internal static class AssemblyLoader {

    internal static Assembly LoadAssembly(string path) => AssemblyLoadContext.Default.LoadFromAssemblyPath(path);

}
