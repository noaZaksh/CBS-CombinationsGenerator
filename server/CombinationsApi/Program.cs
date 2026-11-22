using CombinationsApi.ErrorHandling;
using CombinationsCore.Services;
using CombinationsCore.Storage;
using CombinationsStorage;

const string ClientCorsPolicy = "AngularClient";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Storage: sessions in memory. To move to Redis, register a RedisSessionStore here instead.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ISessionStore, MemorySessionStore>();

// Business logic
builder.Services.AddSingleton<ICombinationsService, CombinationsService>();

// One place that turns exceptions into 400 / 404 / 500 answers
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// Allow the Angular client (a different port) to call the API
var clientUrl = builder.Configuration["ClientUrl"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
    options.AddPolicy(ClientCorsPolicy, policy => policy.WithOrigins(clientUrl).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors(ClientCorsPolicy);
app.MapControllers();

app.Run();
