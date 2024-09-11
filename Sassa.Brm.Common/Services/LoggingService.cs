using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public  class LoggingService(IJSRuntime jsRuntime)
{
    public async Task LogToConsole(string message)
    {
        await jsRuntime.InvokeVoidAsync("console.log", message);
    }
}

