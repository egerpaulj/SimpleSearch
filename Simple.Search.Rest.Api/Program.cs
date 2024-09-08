using Prometheus;
using Simple.Search;
using Simple.Search.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<ISearchService, SearchService>();
builder.Services.AddSingleton<ISearchDataFactory, SearchDataFactory>();
builder.Services.Configure<SearchConfigurationOptions>(
    builder.Configuration.GetSection(SearchConfigurationOptions.SearchConfigurationOptionsKey)
);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.UseRouting();
app.UseHttpMetrics();
app.UseMetricServer();

app.UseDefaultFiles();
app.UseStaticFiles();

Task.Factory.StartNew(() => IndexOnStartup(app));


app.Run();

static void IndexOnStartup(WebApplication app)
{
    var logger = app.Services.GetService<ILoggerFactory>()?.CreateLogger<Program>();
    var lifetime = app.Services.GetService<IHostApplicationLifetime>();
    var searchService = app.Services.GetService<ISearchService>();
    var started = lifetime?.ApplicationStarted.WaitHandle.WaitOne(10000);

    if (started ?? false)
        logger?.LogInformation("Web Application started and data indexed");
    else
        logger?.LogWarning("Web Application Startup timed-out. Attempting to index anyway");

    searchService?.CreateIndex();
}
