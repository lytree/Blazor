using System;
using System.Linq;
using System.Reflection;

namespace Blazor.Hybrid.Avalonia;

public partial class ResourceUrl {

    public const string LocalScheme = "local";
    public const string CustomScheme = "custom";

    internal const string EmbeddedScheme = "embedded";
    internal const string PathSeparator = "/";

    private const string AssemblyPathSeparator = ";";
    private const char AssemblyVersionSeparator = '-';
    private const string AssemblyPrefix = "assembly:";
    private const string DefaultDomain = "webview{0}";
    
    private string Url { get; }

    public ResourceUrl(params string[] path) {
        Url = string.Join("/", path);
    }

    public ResourceUrl(Assembly assembly, params string[] path) : this(path) {
        var identity = assembly.GetName();
        var assemblyName = identity.Name;
        var assemblyVersion = identity.Version is { } version ? $"{AssemblyVersionSeparator}{version}" : "";

        if (Url.StartsWith(PathSeparator)) {
            // only prefix with assembly if necessary, to avoid having the same resource loaded from multiple locations
            Url = AssemblyPrefix + assemblyName + assemblyVersion + AssemblyPathSeparator + Url.Substring(1);
        } else {
            Url = assemblyName + assemblyVersion + PathSeparator + Url;
        }
        Url = BuildUrl(EmbeddedScheme, Url);
    }

    internal ResourceUrl(string scheme, string path) {
        Url = BuildUrl(scheme, path);
    }

    private static string BuildUrl(string scheme, string path) {
        return scheme + Uri.SchemeDelimiter + CombinePath(DefaultDomain, path);
    }

    private static string CombinePath(string path1, string path2) {
        return path1 + (path1.EndsWith(PathSeparator) ? "" : PathSeparator)  + (path2.StartsWith(PathSeparator) ? path2.Substring(1) : path2);
    }

    public override string ToString() {
        return string.Format(Url, "");
    }

    private static bool ContainsAssemblyLocation(Uri url) {
        return url.Scheme == EmbeddedScheme && url.AbsolutePath.StartsWith(PathSeparator + AssemblyPrefix);
    }

    /// <summary>
    /// Supported syntax:
    /// embedded://webview/assembly:AssemblyName;Path/To/Resource
    /// embedded://webview/assembly:AssemblyName-AssemblyVersion;Path/To/Resource
    /// embedded://webview/AssemblyName/Path/To/Resource (AssemblyName is also assumed as default namespace)
    /// embedded://webview/AssemblyName-AssemblyVersion/Path/To/Resource
    /// </summary>
    internal static string[] GetEmbeddedResourcePath(Uri resourceUrl) {
        if (ContainsAssemblyLocation(resourceUrl)) {
            var indexOfPath = resourceUrl.AbsolutePath.IndexOf(AssemblyPathSeparator);
            return resourceUrl.AbsolutePath.Substring(indexOfPath + 1).Split(new [] { PathSeparator }, StringSplitOptions.None);
        }
        var uriParts = resourceUrl.Segments.Select(p => p.Replace(PathSeparator, "")).ToArray();
        var (assemblyName, _) = GetAssemblyNameAndVersion(uriParts[1]);
        return uriParts.Skip(2).Prepend(assemblyName).ToArray();
    }

    /// <summary>
    /// Supported syntax:
    /// embedded://webview/assembly:AssemblyName;Path/To/Resource
    /// embedded://webview/assembly:AssemblyName-AssemblyVersion;Path/To/Resource
    /// embedded://webview/AssemblyName/Path/To/Resource (AssemblyName is also assumed as default namespace)
    /// embedded://webview/AssemblyName-AssemblyVersion/Path/To/Resource
    /// </summary>
    public static (string, Version) GetEmbeddedResourceAssemblyNameAndVersion(Uri resourceUrl) {
        if (ContainsAssemblyLocation(resourceUrl)) {
            var resourcePath = resourceUrl.AbsolutePath.Substring((PathSeparator + AssemblyPrefix).Length);
            var indexOfPath = Math.Max(0, resourcePath.IndexOf(AssemblyPathSeparator));
            return GetAssemblyNameAndVersion(resourcePath.Substring(0, indexOfPath));
        }
        if (resourceUrl.Segments.Length > 1) {
            var assemblySegment = resourceUrl.Segments[1];
            // default assembly name to the first path
            return GetAssemblyNameAndVersion(assemblySegment.EndsWith(PathSeparator) ? assemblySegment.Substring(0, assemblySegment.Length - PathSeparator.Length) : assemblySegment);
        }
        return (string.Empty, null);
    }

    private static (string, Version) GetAssemblyNameAndVersion(string assemblyNameAndVersion) {
        var parts = assemblyNameAndVersion.Split(AssemblyVersionSeparator);
        return parts.Length == 2 ?
            (parts[0], new Version(parts[1])) :
            (parts[0], null);
    }

    internal string WithDomain(string domain) {
        return string.Format(Url, string.IsNullOrEmpty(domain) ? "" : ("_" + domain));
    }
}
