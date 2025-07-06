using Elsa.QuizSocket;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();

// Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

// Register services
builder.Services.AddSingleton<IRedisSubscriptionService, RedisSubscriptionService>();
builder.Services.AddSingleton<IQuizConnectionManager, QuizConnectionManager>();

// CORS configuration
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
app.UseRouting();

app.MapHub<QuizHub>("/quizHub");

// Start Redis subscription service
var redisSubscriptionService = app.Services.GetRequiredService<IRedisSubscriptionService>();
_ = Task.Run(redisSubscriptionService.StartAsync);

app.Run();