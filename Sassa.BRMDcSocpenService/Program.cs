using Sassa.BRM.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RawSqlService>();
builder.Services.AddSingleton<JsonFileUtils>();
builder.Services.AddSingleton<TimedService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TimedService>());
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
