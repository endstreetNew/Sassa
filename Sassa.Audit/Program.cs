using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using razor.Components;
using Sassa.Audit.UI;
using Sassa.Audit.Services;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.Services;
using Sassa.Socpen.Data;

var builder = WebApplication.CreateBuilder(args);

string BrmConnection = builder.Configuration.GetConnectionString("BrmConnection")!;
string CsConnection = builder.Configuration.GetConnectionString("CsConnection")!;
//Factory pattern for contexts
//Factory pattern
builder.Services.AddDbContextFactory<ModelContext>(options =>
{
    options.UseOracle(BrmConnection, opt => opt.CommandTimeout(180));
});
builder.Services.AddPooledDbContextFactory<ModelContext>(options => options.UseOracle(BrmConnection));
//DataServices 
builder.Services.AddSingleton<RawSqlService>();
builder.Services.AddScoped<BRMDbService>();
builder.Services.AddScoped<MisFileService>();
builder.Services.AddScoped<ReportDataService>();
builder.Services.AddSingleton<FileService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddSingleton<CsServiceSettings>(c =>
{
    CsServiceSettings csServiceSettings = new CsServiceSettings();
    csServiceSettings.CsConnection = CsConnection;
    csServiceSettings.CsWSEndpoint = builder.Configuration.GetValue<string>("ContentServer:CSWSEndpoint")!;
    csServiceSettings.CsServiceUser = builder.Configuration.GetValue<string>("ContentServer:CSServiceUser")!;
    csServiceSettings.CsServicePass = builder.Configuration.GetValue<string>("ContentServer:CSServicePass")!;
    csServiceSettings.CsDocFolder = $"{builder.Environment.WebRootPath}\\{builder.Configuration.GetValue<string>("Folders:CS")}\\";
    return csServiceSettings;
});
builder.Services.AddScoped<CSService>();
//Common Services
builder.Services.AddSingleton<StaticService>();
//builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<Helper>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddDataGridEntityFrameworkAdapter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
