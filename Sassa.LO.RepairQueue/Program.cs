using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.LO.RepairQueue.UI;
using Sassa.Models;
using Sassa.Services;

var builder = WebApplication.CreateBuilder(args);
string LoConnectionString = builder.Configuration.GetConnectionString("LoConnection")!;
builder.Services.AddDbContextFactory<LoModelContext>(options =>
options.UseOracle(LoConnectionString));
string BrmConnectionString = builder.Configuration.GetConnectionString("BrmConnection")!;
builder.Services.AddDbContextFactory<ModelContext>(options =>
options.UseOracle(BrmConnectionString));
builder.Services.AddScoped<StaticService>();
builder.Services.AddScoped<LoService>();
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
