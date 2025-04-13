using ApiGateway.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Yarp.ReverseProxy.Swagger;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.GetSection("ReverseProxy");

Console.WriteLine($"Section: {SerializeConfiguration(configuration)}");

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(configuration)
    .RegisterSwagger(configuration)
    ;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        b => b.WithOrigins("http://localhost:4173", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.WebHost.UseKestrelHttpsConfiguration();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var config = app.Services.GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>().CurrentValue;
    options.ConfigureSwaggerEndpoints(config);
});

app.UseCors("AllowLocalhost");

app.MapReverseProxy();

await app.RunAsync();

JToken SerializeConfiguration(IConfiguration config)
{
    var obj = new JObject();
    foreach (var child in config.GetChildren())
    {
        if (child.Path.EndsWith(":0"))
        {
            var arr = new JArray();

            foreach (var arrayChild in config.GetChildren())
            {
                arr.Add(SerializeConfiguration(arrayChild));
            }

            return arr;
        }

        obj.Add(child.Key, SerializeConfiguration(child));
    }

    if (obj.HasValues || config is not IConfigurationSection section) return obj;
    
    // Allow for json that has been embeded as a string in a single key
    if (section.Value.StartsWith('{') && section.Value.EndsWith('}'))
    {
        obj = JObject.Parse(section.Value);
        return obj;
    }

    return ParseJValue(section.Value);
}

JValue ParseJValue(string value)
{
    if (bool.TryParse(value, out var boolean))
        return new JValue(boolean);

    if (long.TryParse(value, out var integer))
        return new JValue(integer);
    
    if (decimal.TryParse(value, out var real))
        return new JValue(real);
    
    return new JValue(value);
}