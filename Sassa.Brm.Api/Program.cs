using Microsoft.EntityFrameworkCore;
using Sassa.BRM.Models;
using Sassa.BRM.Api.Services;


var builder = WebApplication.CreateBuilder(args);


string BrmConnectionString = builder.Configuration.GetConnectionString("BrmConnection")!;
// Add services to the container.
//Factory pattern
builder.Services.AddDbContextFactory<ModelContext>(options =>
options.UseOracle(BrmConnectionString));
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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