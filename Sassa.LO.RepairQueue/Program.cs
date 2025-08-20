using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.BRM.Services;
using Sassa.LO.RepairQueue.UI;
using Sassa.Models;
using Sassa.Services;

var builder = WebApplication.CreateBuilder(args);
string BrmConnectionString = builder.Configuration.GetConnectionString("BrmConnection")!;
string LoConnectionString = builder.Configuration.GetConnectionString("LoConnection")!;
string CsConnectionString = builder.Configuration.GetConnectionString("CsConnection")!;
builder.Services.AddDbContextFactory<ModelContext>(options =>
options.UseOracle(BrmConnectionString));
builder.Services.AddDbContextFactory<LoModelContext>(options =>
options.UseOracle(LoConnectionString));

builder.Services.AddScoped<StaticService>();
builder.Services.AddScoped<FasttrackService>();
builder.Services.AddScoped<LoService>();
builder.Services.AddSingleton<CsServiceSettings>(c =>
{
    CsServiceSettings csServiceSettings = new CsServiceSettings();
    csServiceSettings.CsConnection = CsConnectionString;
    csServiceSettings.CsWSEndpoint = builder.Configuration.GetValue<string>("ContentServer:CSWSEndpoint")!;
    csServiceSettings.CsServiceUser = builder.Configuration.GetValue<string>("ContentServer:CSServiceUser")!;
    csServiceSettings.CsServicePass = builder.Configuration.GetValue<string>("ContentServer:CSServicePass")!;
    csServiceSettings.CsDocFolder = $"{builder.Environment.WebRootPath}\\{builder.Configuration.GetValue<string>("Folders:CS")}\\";
    return csServiceSettings;
});
builder.Services.AddScoped<CSService>();
builder.Services.AddScoped<CoverSheetService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
