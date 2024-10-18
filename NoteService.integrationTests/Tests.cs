using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NoteService.integrationTests;

public sealed class Tests : IClassFixture<WebApplicationFactory<Program>>
{
    
    [Fact]
    public async Task Get_Information_From_Database_Endpoint()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(host => {
                // host.UseSetting(
                //     "NoteServiceOptions:ConnectionString", 
                //     "Host=localhost;Port=5432;Database=platform_db;Username=admin;Password=12345678;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;"
                //);
            });
        var client = factory.CreateClient();
        var actual = await client.GetFromJsonAsync<int>("/users");
        Assert.Equal(expected: 10, actual);
    }
}