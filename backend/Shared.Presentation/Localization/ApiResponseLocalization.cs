using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Shared.Presentation.Localization;

public static class ApiResponseLocalization
{
    public static void Apply(HttpResponse response, string locale)
    {
        response.Headers[HeaderNames.ContentLanguage] = locale;
        AddAcceptLanguageVary(response);
    }

    public static void AddAcceptLanguageVary(HttpResponse response)
    {
        var varyValues = response.Headers[HeaderNames.Vary];
        var tokens = new List<string>();

        foreach (var value in varyValues)
        {
            if (value is null)
            {
                continue;
            }

            foreach (var token in value.Split(',', StringSplitOptions.TrimEntries))
            {
                if (token.Length == 0)
                {
                    continue;
                }

                if (token.Equals("*", StringComparison.Ordinal))
                {
                    response.Headers[HeaderNames.Vary] = "*";
                    return;
                }

                if (!tokens.Contains(token, StringComparer.OrdinalIgnoreCase)) tokens.Add(token);
            }
        }

        if (!tokens.Contains(HeaderNames.AcceptLanguage, StringComparer.OrdinalIgnoreCase))
        {
            tokens.Add(HeaderNames.AcceptLanguage);
        }

        response.Headers[HeaderNames.Vary] = string.Join(", ", tokens);
    }
}
