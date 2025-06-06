using Microsoft.JSInterop;
public class LoggingService(IJSRuntime jsRuntime)
{
    public async Task LogToConsole(string message)
    {
        await jsRuntime.InvokeVoidAsync("console.log", message);
    }
}

