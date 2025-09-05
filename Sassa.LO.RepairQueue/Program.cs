using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.LO.RepairQueue.Services;
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
// Add services to the container.
builder.Services.AddScoped<StaticService>();
builder.Services.AddScoped<LoService>();
builder.Services.AddSingleton<DocumentService>(sp =>
{
    var root = builder.Configuration["Urls:ScanFolderRoot"]; // fallback
    root = root ?? throw new InvalidOperationException("Urls:ScanFolderRoot missing.");
    return new DocumentService(root);
});
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.Configuration["BaseAddress"] ?? "http://localhost:5273") });
builder.Services.AddControllers();
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
app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
