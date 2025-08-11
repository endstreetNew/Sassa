using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Api.Services;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Api.Services;
using Sassa.BRM.Models;
using Sassa.Models;
using Sassa.Services;


var builder = WebApplication.CreateBuilder(args);


string BrmConnectionString = builder.Configuration.GetConnectionString("BrmConnection")!;
string LoConnectionString = builder.Configuration.GetConnectionString("LoConnection")!;
string CsConnectionString = builder.Configuration.GetConnectionString("CsConnection")!;
// Add services to the container.
//Factory pattern
builder.Services.AddDbContextFactory<ModelContext>(options =>
options.UseOracle(BrmConnectionString));
builder.Services.AddDbContextFactory<LoModelContext>(options =>
options.UseOracle(LoConnectionString));
builder.Services.AddScoped<StaticService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<ApplicationService>();
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
builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(builder.Configuration.GetValue<int>("Urls:AppPort")); // HTTP
                                                                                    // serverOptions.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
});
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();