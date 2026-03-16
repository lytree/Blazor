using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Shared.Core;


public sealed class BlazorEmbeddedFileProvider : IFileProvider
{
    private const string Content = "_content/";
    private const string ContentDevToysBlazor = "_content/Blazor.Shared";

    private readonly EmbeddedFileProvider _embeddedFileProvider
        = new(typeof(BlazorEmbeddedFileProvider).Assembly, baseNamespace: string.Empty);

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        if (subpath.StartsWith(ContentDevToysBlazor))
        {
            subpath = subpath.Substring(Content.Length);
            return _embeddedFileProvider.GetDirectoryContents(subpath);
        }

        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        if (subpath.StartsWith(ContentDevToysBlazor))
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
