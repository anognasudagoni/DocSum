using DocSumRepository;
using DocSumServices;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDocSumRepo, DocSumRepo>();

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddScoped<IDocSumService>((provider) =>
{
    var textAnalyticsEndpoint = configuration["TextAnalytics:Endpoint"];
    var textAnalyticsKey = configuration["TextAnalytics:Key"];
    var docSumRepo = provider.GetRequiredService<IDocSumRepo>();
    return new DocSumService(docSumRepo, textAnalyticsEndpoint, textAnalyticsKey);
});

builder.Services.AddSingleton<CosmosClient>((provider) =>
{
    var endpointUri = configuration["CosmosDbSettings:EndpointUri"];
    var primaryKey = configuration["CosmosDbSettings:PrimaryKey"];
    var cosmosClientOptions = new CosmosClientOptions
    {
        ApplicationName = "YourAppName"
    };
    var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });
    var cosmosClient = new CosmosClient(endpointUri, primaryKey, cosmosClientOptions);
    cosmosClient.ClientOptions.ConnectionMode = ConnectionMode.Direct;
    return cosmosClient;
});

var app = builder.Build();

// Configure CORS
app.UseCors(builder =>
{
    builder.AllowAnyOrigin() // Allow access from all origins
           .AllowAnyHeader()
           .AllowAnyMethod();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();