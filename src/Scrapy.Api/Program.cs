using System.Net;
using NodaTime;
using Polly;
using Polly.Extensions.Http;
using Scrapy.Adapters.TvMaze;
using Scrapy.Adapters.TvMaze.Configurations;
using Scrapy.Api.Middlewares;
using Scrapy.Jobs.HostedServices;
using Scrapy.Jobs.Models;
using Scrapy.Jobs.Services;
using Scrapy.Jobs.Services.Interfaces;
using Scrapy.UseCases;
using Scrapy.UseCases.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDistributedMemoryCache();

var tvMazeSettings = builder.Configuration.GetSection("TvMazeSettings").Get<TvMazeSettings>();

builder.Services.AddTvMazeAdapter(tvMazeSettings).AddPolicyHandler(GetHttpPolicies());
builder.Services.AddSingleton<IClock>(SystemClock.Instance);

builder.Services.AddTransient<IGetShowsUseCase, GetShowsUseCase>();
builder.Services.AddTransient<IScrapperService, ScrapperService>();

if (builder.Configuration.GetValue<bool>("SchedulingSettings:Enabled"))
{
    builder.Services.Configure<ScheduleConfig>(builder.Configuration.GetSection("SchedulingSettings"));
    builder.Services.AddHostedService<ShowScrapperHostedService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "app v1"));
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// If API returns "TooManyRequests" error then simply wait for somtime and then retry 
static IAsyncPolicy<HttpResponseMessage> GetHttpPolicies()
{
    var randomExtraWait = new Random();

    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
