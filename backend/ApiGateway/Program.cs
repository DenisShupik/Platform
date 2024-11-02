var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        b => b.WithOrigins("https://localhost:4173","https://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.WebHost.UseKestrelHttpsConfiguration();

var app = builder.Build();

app.UseCors("AllowLocalhost");

app.MapReverseProxy();

await app.RunAsync();
