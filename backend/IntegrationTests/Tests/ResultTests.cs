using System.Runtime.CompilerServices;
using System.Text.Json;
using Shared.Domain.Abstractions.Errors;
using Shared.Domain.Abstractions.Results;
using Shared.Presentation.Convertors;
using Shared.Presentation.Extensions;

namespace IntegrationTests.Tests;

public sealed class ResultTests
{
    [Test]
    public async Task Result_ExposesValueWithoutAllocatingFailure()
    {
        Result<string, FirstError, SecondError> result = "value";

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.TryGetValue(out var value, out _)).IsTrue();
        await Assert.That(value).IsEqualTo("value");
        await Assert.That(result.TryGetFailure(out _)).IsFalse();
    }

    [Test]
    public async Task SuccessOr_ExposesTypedFailure()
    {
        SuccessOr<FirstError, SecondError> result = new SecondError();

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.TryGetFailure(out var failure)).IsTrue();
        await Assert.That(failure.TryGet<FirstError>(out _)).IsFalse();
        await Assert.That(failure.TryGet<SecondError>(out var error)).IsTrue();
        await Assert.That(error).IsNotNull();
    }

    [Test]
    public async Task Failure_PropagatesIntoCompatibleSuccessOr()
    {
        SuccessOr<SecondError, ThirdError> inner = new ThirdError();
        _ = inner.TryGetFailure(out var failure);

        SuccessOr<FirstError, SecondError, ThirdError> outer = failure;

        await Assert.That(outer.TryGetFailure(out var outerFailure)).IsTrue();
        await Assert.That(outerFailure.TryGet<ThirdError>(out _)).IsTrue();
    }

    [Test]
    public async Task DefaultUnion_IsRejectedInsteadOfLookingSuccessful()
    {
        var result = default(Result<string, FirstError>);
        var successOr = default(SuccessOr<FirstError>);

        await Assert.That(() => result.IsSuccess).Throws<InvalidOperationException>();
        await Assert.That(() => successOr.IsSuccess).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task SingleErrorSuccessOr_UsesOneMachineWord()
    {
        await Assert.That(Unsafe.SizeOf<SuccessOr<FirstError>>()).IsEqualTo(IntPtr.Size);
        await Assert.That(Unsafe.SizeOf<Failure<FirstError>>()).IsEqualTo(IntPtr.Size);
        await Assert.That(Unsafe.SizeOf<Result<object, FirstError>>()).IsEqualTo(IntPtr.Size * 2);
    }

    [Test]
    public async Task ResultJsonConverterFactory_SupportsEveryGeneratedArity()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .ApplyApiContractOptions()
            .ApplyApiContractOptions();
        var factory = options.Converters.OfType<ResultJsonConverterFactory>().Single();

        var supportedTypes = new[]
        {
            typeof(Result<string, FirstError>),
            typeof(Result<string, FirstError, SecondError>),
            typeof(Result<string, FirstError, SecondError, ThirdError>),
            typeof(Result<string, FirstError, SecondError, ThirdError, FourthError>),
            typeof(Result<string, FirstError, SecondError, ThirdError, FourthError, FifthError>),
            typeof(Result<string, FirstError, SecondError, ThirdError, FourthError, FifthError, SixthError>),
            typeof(Result<string, FirstError, SecondError, ThirdError, FourthError, FifthError, SixthError,
                SeventhError>)
        };

        await Assert.That(supportedTypes.All(factory.CanConvert)).IsTrue();
    }

    [Test]
    public async Task GeneratedResultJsonConverter_RoundTripsValueAndFailures()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            .ApplyApiContractOptions();
        Result<string, FirstError> valueResult = "value";
        Result<string, FirstError> singleFailureResult = new FirstError();
        Result<string, FirstError, SecondError, ThirdError, FourthError, FifthError, SixthError,
            SeventhError> failureResult = new SeventhError();

        var valueJson = JsonSerializer.Serialize(valueResult, options);
        var singleFailureJson = JsonSerializer.Serialize(singleFailureResult, options);
        var failureJson = JsonSerializer.Serialize(failureResult, options);
        var restoredValue = JsonSerializer.Deserialize<Result<string, FirstError>>(valueJson, options);
        var restoredSingleFailure = JsonSerializer.Deserialize<Result<string, FirstError>>(
            singleFailureJson,
            options);
        var restoredFailure = JsonSerializer.Deserialize<Result<string, FirstError, SecondError, ThirdError,
            FourthError, FifthError, SixthError, SeventhError>>(failureJson, options);

        await Assert.That(restoredValue.TryGetValue(out var value)).IsTrue();
        await Assert.That(value).IsEqualTo("value");
        await Assert.That(restoredSingleFailure.TryGetFailure(out var singleFailure)).IsTrue();
        await Assert.That(singleFailure.TryGet<FirstError>(out _)).IsTrue();
        await Assert.That(restoredFailure.TryGetFailure(out var failure)).IsTrue();
        await Assert.That(failure.TryGet<SeventhError>(out _)).IsTrue();
    }

    private sealed record FirstError : Error;
    private sealed record SecondError : Error;
    private sealed record ThirdError : Error;
    private sealed record FourthError : Error;
    private sealed record FifthError : Error;
    private sealed record SixthError : Error;
    private sealed record SeventhError : Error;
}
