using System.ComponentModel.Composition;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Blazor.Core.Web;

[Export(typeof(IWebClientService))]
internal sealed class WebClientService : IWebClientService
{
    private readonly ILogger _logger = NullLogger.Instance;
    private readonly HttpClient _httpClient = new();

    [ImportingConstructor]
    public WebClientService()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DevToys");
    }

    public async Task<string?> SafeGetStringAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetStringAsync(uri, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching data from {uri}", uri);
            return null;
        }
    }
}
