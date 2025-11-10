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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

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
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IRequestHandler<GetNewsQuery, IReadOnlyList<NewsItem>>, GetNewsQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetQuartzJobsQuery, IReadOnlyList<QuartzJobInfo>>, GetQuartzJobsFromApiHandler>();

var app = builder.Build();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


