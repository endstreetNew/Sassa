using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Sassa.Audit.Components;
using Sassa.Audit.Services;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;

var builder = WebApplication.CreateBuilder(args);

string BrmConnection = builder.Configuration.GetConnectionString("BrmConnection")!;
//Factory pattern for contexts
builder.Services.AddPooledDbContextFactory<ModelContext>(options => options.UseOracle(BrmConnection));
//DataServices 
builder.Services.AddSingleton<RawSqlService>();
builder.Services.AddScoped<BRMDbService>();
builder.Services.AddScoped<MisFileService>();
builder.Services.AddScoped<ReportDataService>();
builder.Services.AddSingleton<FileService>();
//Common Services
builder.Services.AddSingleton<StaticService>();
//builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<Helper>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

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
