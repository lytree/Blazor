
using System.Text;
using Blazor.Hybrid.Shared;
using Blazor.Shared.Core;
using Gdk;
using GLib;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Blazor.Hybrid.Linux;

internal sealed partial class Clipboard : IClipboard
{
    private readonly ILogger<IClipboard> _logger;
    private readonly IFileStorage _fileStorage;

    public Clipboard(IFileStorage fileStorage, ILogger<IClipboard> logger)
    {
        _logger = logger;
        _fileStorage = fileStorage;
    }

    public async Task<object?> GetClipboardDataAsync()
    {
        try
        {
            FileInfo[]? files = await GetClipboardFilesAsync();
            if (files is not null)
            {
                return files;
            }

            string? text = await GetClipboardTextAsync();
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }

            Image<Rgba32>? image = await GetClipboardImageAsync();
            if (image is not null)
            {
                return image;
            }
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }

        return null;
    }

    public async Task<FileInfo[]?> GetClipboardFilesAsync()
    {
        try
        {
            string? clipboardContentText = await GetClipboardTextAsync();
            if (clipboardContentText is not null)
            {
                string[] filePaths = clipboardContentText.Split('\n');
                var files = new List<FileInfo>();
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        files.Add(new FileInfo(filePath));
                    }
                    else
                    {
                        return null;
                    }
                }

                return [.. files];
            }
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }

        return null;
    }

    public async Task<string?> GetClipboardTextAsync()
    {
        try
        {
            Gdk.Clipboard clipboard = Gdk.Display.GetDefault()!.GetClipboard();
            return await clipboard.ReadTextAsync();
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }

        return null;
    }

    public async Task<Image<Rgba32>?> GetClipboardImageAsync()
    {
        try
        {
            Gdk.Clipboard clipboard = Gdk.Display.GetDefault()!.GetClipboard();
            var tcs = new TaskCompletionSource<Image<Rgba32>?>();
            try
            {
                var callbackHandler = Gio.AsyncResultHelper.NewFromPointer(clipboard.Handle.DangerousGetHandle(), false);

                var texture = clipboard.ReadTextureFinish(callbackHandler);
                if (texture is not null)
                {
                    string tempFile = Path.GetTempFileName();
                    texture.SaveToPng(tempFile);

                    using Image image = await Image.LoadAsync(tempFile);
                    tcs.SetResult(image.CloneAs<Rgba32>(image.Configuration));

                    File.Delete(tempFile);
                    texture.Dispose();
                }
            }
            catch (GException)
            {
                tcs.SetResult(null);
            }
            return await tcs.Task;
        }
        catch (Exception ex)
        {
            LogGetClipboardFailed(ex);
        }

        return null;
    }

    public async Task SetClipboardImageAsync(Image? image)
    {
        Gdk.Clipboard clipboard = Gdk.Display.GetDefault()!.GetClipboard();

        if (image is not null)
        {
            var encoder = new PngEncoder
            {
                ColorType = PngColorType.RgbWithAlpha,
                TransparentColorMode = PngTransparentColorMode.Preserve,
                BitDepth = PngBitDepth.Bit8,
                CompressionLevel = PngCompressionLevel.BestSpeed
            };

            var pngMemoryStream = new MemoryStream();
            await image.SaveAsPngAsync(pngMemoryStream, encoder);
            pngMemoryStream.Seek(0, SeekOrigin.Begin);

            using var pngBytes = GLib.Bytes.New(pngMemoryStream.ToArray());
            using var texture = Texture.NewFromBytes(pngBytes);
            clipboard.SetTexture(texture);
        }
        else
        {
            clipboard.SetText(string.Empty);
        }
    }

    public Task SetClipboardFilesAsync(FileInfo[]? filePaths)
    {
        try
        {
            Gdk.Clipboard clipboard = Gdk.Display.GetDefault()!.GetClipboard();

            if (filePaths is not null)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("copy"); // Important.

                for (int i = 0; i < filePaths.Length; i++)
                {
                    stringBuilder.Append($"\nfile://{filePaths[i].FullName}");
                }

                string newClipboardContentString = stringBuilder.ToString();
                byte[] data = Encoding.UTF8.GetBytes(newClipboardContentString);
                using var dataBytes = GLib.Bytes.New(data);
                using var content = ContentProvider.NewForBytes("x-special/gnome-copied-files", dataBytes);
                clipboard.SetContent(content);
            }
            else
            {
                clipboard.SetText(string.Empty);
            }
        }
        catch (Exception ex)
        {
            LogSetClipboardTextFailed(ex);
        }

        return Task.CompletedTask;
    }

    public Task SetClipboardTextAsync(string? data)
    {
        try
        {
            Gdk.Clipboard clipboard = Gdk.Display.GetDefault()!.GetClipboard();
            clipboard.SetText(data ?? string.Empty);
        }
        catch (Exception ex)
        {
            LogSetClipboardTextFailed(ex);
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(0, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    partial void LogGetClipboardFailed(Exception ex);

    [LoggerMessage(1, LogLevel.Error, "Failed to set the clipboard text.")]
    partial void LogSetClipboardTextFailed(Exception ex);
}
