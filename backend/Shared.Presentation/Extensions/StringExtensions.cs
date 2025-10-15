namespace Shared.Presentation.Extensions;

public static class StringExtensions
{
    extension(string input)
    {
        public string ToCamelCase()
        {
            return string.Create(input.Length, input, static (span, str) =>
            {
                str.AsSpan().CopyTo(span);
                span[0] = char.ToLowerInvariant(span[0]);
            });
        }

        public string ToUpperSnakeCase()
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var source = input.AsSpan();

            var underscoreCount = 0;
            var prev = source[0];

            for (var i = 1; i < source.Length; i++)
            {
                var current = source[i];

                if (char.IsUpper(current))
                {
                    if (!char.IsUpper(prev) || i + 1 < source.Length && char.IsLower(source[i + 1]))
                    {
                        underscoreCount++;
                    }
                }

                prev = current;
            }

            var resultLength = source.Length + underscoreCount;

            return string.Create(resultLength, input, static (span, str) =>
            {
                var src = str.AsSpan();
                var writePos = 0;

                span[writePos++] = char.ToUpperInvariant(src[0]);
                var prev = src[0];

                for (var i = 1; i < src.Length; i++)
                {
                    var current = src[i];

                    if (char.IsUpper(current))
                    {
                        if (!char.IsUpper(prev) || (i + 1 < src.Length && char.IsLower(src[i + 1])))
                        {
                            span[writePos++] = '_';
                        }

                        span[writePos++] = current;
                    }
                    else
                    {
                        span[writePos++] = char.ToUpperInvariant(current);
                    }

                    prev = current;
                }
            });
        }
    }
}