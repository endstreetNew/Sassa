using Sassa.BRM.Models;
using Microsoft.EntityFrameworkCore;

namespace Sassa.Activity.Api
{

    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);
            string BrmConnectionString = builder.Configuration.GetConnectionString("BrmConnection")!;
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddDbContextFactory<ModelContext>(options =>
            options.UseOracle(BrmConnectionString));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            //Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
