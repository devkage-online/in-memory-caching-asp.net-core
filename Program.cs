using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.MapGet("/weatherforecast", (IMemoryCache memoryCache) =>
    {
        WeatherForecast[] forecast = [];

        if (!memoryCache.TryGetValue("forecastData", out WeatherForecast[] cacheValue))
        {
            cacheValue = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)],
                DateTime.Now
            ))
            .ToArray();

            // var cacheEntryOptions = new MemoryCacheEntryOptions()
            //     .SetSlidingExpiration(TimeSpan.FromSeconds(15));

            // memoryCache.Set("forecastData", cacheValue, cacheEntryOptions);
            memoryCache.Set("forecastData", cacheValue, TimeSpan.FromSeconds(15));
        }

        forecast = cacheValue.ToArray();

        return forecast;
    }
)
.WithName("GetWeatherForecast")
.WithOpenApi();

// mengambil data dari cache
app.MapGet("/cacheforecast", (IMemoryCache memoryCache) =>
    {
        var cacheEntry = memoryCache.Get<WeatherForecast[]?>("forecastData");
        return cacheEntry!.ToList();
    }
)
.WithName("GetCacheForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary, DateTime CreatedAt)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
