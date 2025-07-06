using Elsa.QuizAPI.Data;
using Elsa.QuizAPI.Features.Quizzes;
using Elsa.QuizAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// L1 cache setting
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 100 * 1024 * 1024;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
});

// L2 cache setting
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();

builder.Services.AddScoped<IQuizManagementService, QuizManagementService>();
builder.Services.AddScoped<IQuizManagementRepository, QuizManagementRepository>();

builder.Services.AddScoped<IUserContext, DumpUserContext>();
builder.Services.AddScoped<IEventPublisher, RedisEventPublisher>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseRouting();
app.MapControllers();

app.Run();