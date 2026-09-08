
using Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using prjMemberAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace prjMemberAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<tempdbContext>(options =>
            options.UseSqlServer(
             builder.Configuration.GetConnectionString("DefaultConnection")
            ));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy
                    .WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowAngular");
            app.MapControllers();

            app.Run();
        }
    }
}
