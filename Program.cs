using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using VN_API.Database;
using VN_API.Services;
using VN_API.Services.Implimentations;
using VN_API.Services.Interfaces;

namespace VN_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddTransient<INovelService, NovelAdderService>();
            builder.Services.AddTransient<IVNDBQueriesService, VNDBQueriesService>();

            builder.Services.AddMemoryCache();

            builder.Services.AddCors();

            builder.Services.AddControllers()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


            builder.Services.AddSingleton<IAmazonS3>(sp =>
            {
                var awsCredentials = new BasicAWSCredentials("10559668e5594a7d9b1a01100bbcb6f2", "9325b57f62bd48fca172f2fb6db4aa33");
                var config = new AmazonS3Config
                {
                    ServiceURL = "https://s3.ru-1.storage.selcloud.ru", // Конечная точка Selectel
                    ForcePathStyle = true // Важно для совместимости с Selectel
                };
                return new AmazonS3Client(awsCredentials, config);
            });


            var connectionString = builder.Configuration.GetConnectionString("VndbConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            //string connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=moloko990;Database=vndb;";
            builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

            }

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}