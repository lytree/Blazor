using System;

namespace Blazor.Hybrid.Avalonia;

internal static class ResourceHandlerExtensions {

    public static void LoadEmbeddedResource(this ResourceHandler resourceHandler, Uri url) {
        var stream = ResourcesManager.TryGetResource(url, true, out string extension);

        if (stream != null) {
            resourceHandler.RespondWith(stream, extension);
        }
    }
}
