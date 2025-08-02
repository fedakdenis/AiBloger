using Microsoft.EntityFrameworkCore;
using MediatR;
using Quartz;
using AiBloger.Infrastructure.Data;
using AiBloger.Infrastructure.Repositories;
using AiBloger.Infrastructure.Services;
using AiBloger.Core.Interfaces;
using AiBloger.Api.Configuration;
using AiBloger.Api.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<NewsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and MediatR
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AiBloger.Core.AssemblyAnchor).Assembly));

// Scraper settings
builder.Services.Configure<NewsScraperOptions>(builder.Configuration.GetSection("NewsScraper"));

// Register scraper services
builder.Services.AddHttpClient<INewsScraperService, NewsScraperService>();
builder.Services.AddScoped<INewsScraperService, NewsScraperService>();

// Register OpenAI service
builder.Services.AddScoped<IAuthorService>(provider =>
{
    var apiKey = builder.Configuration["OpenAI:ApiKey"];
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        throw new InvalidOperationException("OpenAI API key not found in configuration. Add 'OpenAI:ApiKey' to appsettings.json");
    }
    return new OpenAiResponseService(apiKey);
});

// Register Telegram service
builder.Services.AddScoped<IBlogerService>(provider =>
{
    var botToken = builder.Configuration["Telegram:BotToken"];
    var chatId = builder.Configuration["Telegram:ChatId"];
    var logger = provider.GetRequiredService<ILogger<TelegramService>>();
    
    if (string.IsNullOrWhiteSpace(botToken))
    {
        throw new InvalidOperationException("Telegram Bot Token not found in configuration. Add 'Telegram:BotToken' to appsettings.json");
    }
    
    if (string.IsNullOrWhiteSpace(chatId))
    {
        throw new InvalidOperationException("Telegram Chat ID not found in configuration. Add 'Telegram:ChatId' to appsettings.json");
    }
    
    return new TelegramService(botToken, chatId, logger);
});

var newsScraperOptions = builder.Configuration.GetSection("NewsScraper").Get<NewsScraperOptions>();
// Configure Quartz
builder.Services.AddQuartz(q =>
{
    // Configure news scraping job
    var newsJobKey = new JobKey("NewsScrapingJob");
    q.AddJob<PullNewsJob>(opts => opts.WithIdentity(newsJobKey));

    // Get settings to determine interval
    var scrapingInterval = builder.Configuration.GetValue<int>("NewsScraper:ScrapingIntervalMinutes", 30);

    q.AddTrigger(opts => opts
        .ForJob(newsJobKey)
        .WithIdentity("NewsScrapingJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(scrapingInterval)
            .RepeatForever())
        .StartNow());

    // Configure post writing job
    var writePostJobKey = new JobKey("WritePostJob");
    q.AddJob<WritePostJob>(opts => opts.WithIdentity(writePostJobKey));

    q.AddTrigger(opts => opts
        .ForJob(writePostJobKey)
        .WithIdentity("WritePostJob-trigger")
        .WithCronSchedule("0 0 6 * * ?") // Every day at 6:00 GMT (0 seconds, 0 minutes, 6 hours, any day of month, any month, any day of week)
        .StartNow());
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Create database on startup (if not exists)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NewsDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Database created/verified successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating database: {ex.Message}");
    }
}

app.Run();
