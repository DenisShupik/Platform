namespace Shared.Domain.Abstractions.Results;

public interface IResult;

public interface IResult<out TValue> : IResult
    where TValue : notnull;
