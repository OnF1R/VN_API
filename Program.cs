using Amazon.Runtime;
using Amazon.S3;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using VN_API.Data.Keys;
using VN_API.Database;
using VN_API.Middleware;
using VN_API.Services;
using VN_API.Services.Implementations;
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

            builder.Services.AddScoped<INovelService, NovelAdderService>();
            builder.Services.AddTransient<ICommentService, CommentService>();
            builder.Services.AddTransient<ICommentRatingService, CommentRatingService>();
            builder.Services.AddTransient<IVNDBQueriesService, VNDBQueriesService>();

            builder.Services.AddMemoryCache();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        //builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

                        builder.WithOrigins("https://astral-novel.ru")
                               .AllowAnyHeader()
                               .AllowAnyMethod();

                        builder.WithOrigins("https://localhost:7118")
                               .AllowAnyHeader()
                               .AllowAnyMethod();

                        builder.WithOrigins("http://localhost:5000")
                               .AllowAnyHeader()
                               .AllowAnyMethod();

                        builder.WithOrigins("https://www.astral-novel.ru")
                               .AllowAnyHeader()
                               .AllowAnyMethod();

                        builder.WithOrigins("91.107.126.70")
                               .AllowAnyHeader()
                               .AllowAnyMethod();

                        builder.WithOrigins("https://astral-novel.ru")
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });


            builder.Services.AddControllers()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


            builder.Services.AddSingleton<IAmazonS3>(sp =>
            {
                var awsCredentials = new BasicAWSCredentials(SelecterS3Keys.AccessKey, SelecterS3Keys.SecretKey);
                var config = new AmazonS3Config
                {
                    ServiceURL = "https://s3.ru-1.storage.selcloud.ru",
                    ForcePathStyle = true
                };
                return new AmazonS3Client(awsCredentials, config);
            });

            builder.Services.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Period = "1m",
                        Limit = 300
                    }
                };
            });

            builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            var connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=moloko990;Database=vndb;";
            //var connectionString = "Server=91.107.126.70;Port=5432;User Id=postgres;Password=moloko990;Database=vndb;";
            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'VndbConnection' not found.");

            if (connectionString == null)
            {
                Console.WriteLine("Connection string 'VndbConnection' not found.");
                throw new InvalidOperationException("Connection string 'VndbConnection' not found.");
            }
            else
            {
                Console.WriteLine($"Connection string: {connectionString}");
            }

            builder.Services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseNpgsql(connectionString);
                //options.EnableSensitiveDataLogging();
            });

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

            app.UseCors("AllowSpecificOrigin");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //app.UseMiddleware<IpRestrictionMiddleware>();

            app.UseIpRateLimiting();

            app.MapControllers();

            app.Run();
        }
    }
}