using XlsxImport.Services;
using XlxsImport.Components;
using XlxsImport.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<RawSqlService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<XlsxHelper>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
