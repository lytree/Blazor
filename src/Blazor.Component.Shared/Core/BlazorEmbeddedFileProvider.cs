using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Blazor.Shared.Core;


public sealed class BlazorEmbeddedFileProvider : IFileProvider
{
    private const string Content = "_content/";
    private const string ContentBlazorShared = "_content/Blazor.Shared";

    private readonly EmbeddedFileProvider _embeddedFileProvider
        = new(typeof(BlazorEmbeddedFileProvider).Assembly, baseNamespace: string.Empty);

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        if (subpath.StartsWith(ContentBlazorShared))
        {
            subpath = subpath.Substring(Content.Length);
            return _embeddedFileProvider.GetDirectoryContents(subpath);
        }

        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        if (subpath.StartsWith(ContentBlazorShared))
        {
            subpath = subpath.Substring(Content.Length);
            return _embeddedFileProvider.GetFileInfo(subpath);
        }

        string name = Path.GetFileName(subpath);
        return new NotFoundFileInfo(name);
    }

    public IChangeToken Watch(string filter)
    {
        return _embeddedFileProvider.Watch(filter);
    }
}
