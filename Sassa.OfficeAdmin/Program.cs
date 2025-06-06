using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.FluentUI.AspNetCore.Components;
using Sassa.Brm.Common.Helpers;
using Sassa.OfficeAdmin.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();
builder.Services.AddAuthenticationServices();
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});
//Connection strings   
string BrmConnection = builder.Configuration.GetConnectionString("BrmConnection")!;
//Factory pattern for contexts
builder.Services.AddPooledContextServices(BrmConnection);

builder.Services.AddScoped<SocpenService>();
builder.Services.AddSessionServices();
builder.Services.AddScoped<Helper>();

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
