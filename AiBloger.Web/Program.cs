using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AiBloger.Web.Services;
using Microsoft.EntityFrameworkCore;
using AiBloger.Infrastructure.Data;
using AiBloger.Infrastructure.Repositories;
using AiBloger.Core.Interfaces;
using AiBloger.Core.Mediator;
using AiBloger.Core.Handlers;
using AiBloger.Core.Queries;
using AiBloger.Core.Entities;
using AiBloger.Web.Handlers;
using AiBloger.Infrastructure.Services;
using AiBloger.Core.Commands;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

// API client
builder.Services.AddHttpClient<IQuartzSchedulerApi, QuartzSchedulerApi>(client =>
{
	var baseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5001";
	client.BaseAddress = new Uri(baseUrl);
});

// Core/Infra for in-process queries
builder.Services.AddDbContext<NewsDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<ISourceRepository, SourceRepository>();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IRequestHandler<GetNewsQuery, IReadOnlyList<NewsItem>>, GetNewsQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetQuartzJobsQuery, IReadOnlyList<QuartzJobInfo>>, GetQuartzJobsFromApiHandler>();
builder.Services.AddScoped<IRequestHandler<GeneratePostPreviewCommand, PostInfo>, GeneratePostPreviewCommandHandler>();
builder.Services.AddScoped<IRequestHandler<GetSourcesQuery, IReadOnlyList<Source>>, GetSourcesQueryHandler>();
builder.Services.AddScoped<IRequestHandler<AddSourcesBatchCommand, int>, AddSourcesBatchCommandHandler>();
builder.Services.AddScoped<IModelCatalog>(provider =>
{
	var apiKey = builder.Configuration["OpenAI:ApiKey"];
	var cache = provider.GetRequiredService<IMemoryCache>();
	var logger = provider.GetRequiredService<ILogger<OpenAIModelCatalog>>();
	if (string.IsNullOrWhiteSpace(apiKey))
	{
		throw new InvalidOperationException("OpenAI API key not found in configuration. Add 'OpenAI:ApiKey' to appsettings.json");
	}
	return new OpenAIModelCatalog(apiKey, cache, logger);
});
builder.Services.AddScoped<IAuthorService>(provider =>
{
	var apiKey = builder.Configuration["OpenAI:ApiKey"];
	if (string.IsNullOrWhiteSpace(apiKey))
	{
		throw new InvalidOperationException("OpenAI API key not found in configuration. Add 'OpenAI:ApiKey' to appsettings.json");
	}
	return new OpenAiResponseService(apiKey);
});

var app = builder.Build();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


