using System.Net;
using Amazon.Runtime;

namespace FileService.Presentation.Factories;

public sealed class SSLFactory : HttpClientFactory
{
    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {
        var httpMessageHandler = CreateClientHandler();
        if (clientConfig.MaxConnectionsPerServer.HasValue)
            httpMessageHandler.MaxConnectionsPerServer = clientConfig.MaxConnectionsPerServer.Value;
        httpMessageHandler.AllowAutoRedirect = clientConfig.AllowAutoRedirect;

        httpMessageHandler.AutomaticDecompression = DecompressionMethods.None;

        var proxy = clientConfig.GetWebProxy();
        if (proxy != null)
        {
            httpMessageHandler.Proxy = proxy;
        }

        if (httpMessageHandler.Proxy != null && clientConfig.ProxyCredentials != null)
        {
            httpMessageHandler.Proxy.Credentials = clientConfig.ProxyCredentials;
        }

        var httpClient = new HttpClient(httpMessageHandler);

        if (clientConfig.Timeout.HasValue)
        {
            httpClient.Timeout = clientConfig.Timeout.Value;
        }

        return httpClient;
    }

    private static HttpClientHandler CreateClientHandler() =>
        new()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
}