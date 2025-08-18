using EA.Model.Models.MyLibraryDB;
using EA.Model.SpDbContext;
using EA.Services.Repositories.Implementation;
using EA.Services.Repositories.Interface;
using EvaluationAPI.Helper;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using Serilog;

namespace EvaluationAPI;

public class Program
{
    public static void Main(string[] args)
    {
         var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        
        //Code for adding the log to the new file 
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();
        
        //code for adding the connectionString
        var connectionString = builder.Configuration.GetConnectionString("DBConnection");
        
             //DBContext
        builder.Services.AddDbContext<LibraryDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {

            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);
        
        //SPCONTEXT CONFIGURATION
        builder.Services.AddDbContext<LibraryManagementSpContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {

                sqlOptions.EnableRetryOnFailure();

            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(true);
        }, ServiceLifetime.Transient);
        
        // Register the UserService AND Unit OF Work
        UnitOfWorkServiceCollectionExtentions.AddUnitOfWork<LibraryDbContext>(builder.Services);
        
        builder.Services.AddScoped<IBookRepository , BookRepository>();

        
        builder.Services.AddControllers()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<BookRequestModelValidator>());
        builder.Services.AddControllers()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<BookRequestWithoutSidModelValidator>());
        // Add services to the container.

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

        app.UseHttpsRedirection();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthorization();


        app.MapControllers();
       
        app.Run();
    }
}