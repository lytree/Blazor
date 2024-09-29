using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;
namespace Blazor.Hybrid.Core;

internal sealed class BlazorSandboxedFileReader : SandboxedFileReader
{
    private readonly OneOf<IBrowserFile, FileInfo> _file;
    private readonly IFileStorage? _fileStorage;

    public BlazorSandboxedFileReader(IBrowserFile browserFile)
        : base(browserFile.Name)
    {
        _file = OneOf<IBrowserFile, FileInfo>.FromT0(browserFile);
        _fileStorage = null;
    }

    public BlazorSandboxedFileReader(FileInfo file, IFileStorage fileStorage)
        : base(file.Name)
    {        
        _file = file;
        _fileStorage = fileStorage;
    }

    protected override ValueTask<Stream> OpenReadFileAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_file.Match(
            (IBrowserFile browserFile) => browserFile.OpenReadStream(browserFile.Size, cancellationToken),
            (FileInfo fileInfo) =>
            {
                return _fileStorage.OpenReadFile(fileInfo.FullName);
            }));
    }
}
