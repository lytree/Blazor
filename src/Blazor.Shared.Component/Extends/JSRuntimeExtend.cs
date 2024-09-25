using Microsoft.JSInterop;

namespace Blazor.Shared.Extends;

public static class JSRuntimeExtend
{
    public static async Task<T> InvokeFunctionAsync<T>(this IJSRuntime jsRuntime, string code, params object[] args)
    {
        var guid = Guid.NewGuid().ToString();
        await jsRuntime.InvokeVoidAsync("H.AddFunction", guid, code);
        return await jsRuntime.InvokeAsync<T>($"H.Functions.{guid}", args);
    }

    public static async Task<System.Text.Json.JsonElement> InvokeFunctionAsync(this IJSRuntime jsRuntime, string code, params object[] args)
    {
        return await jsRuntime.InvokeFunctionAsync<System.Text.Json.JsonElement>(code, args);
    }

    public static async Task InvokeFunctionVoidAsync(this IJSRuntime jsRuntime, string code, params object[] args)
    {
        var guid = Guid.NewGuid().ToString();
        await jsRuntime.InvokeVoidAsync("H.AddFunction", guid, code);
        await jsRuntime.InvokeVoidAsync($"H.Functions.{guid}", args);
    }
}