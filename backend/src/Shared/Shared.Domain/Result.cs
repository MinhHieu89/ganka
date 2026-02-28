namespace Shared.Domain;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Uses the Railway Oriented Programming pattern -- no exceptions for expected failures.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value of type T.
/// Accessing Value on a failure or Error on a success throws InvalidOperationException.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value on a failed result. Check IsSuccess first.");

    private Result(T value) : base(true, Error.None)
    {
        _value = value;
    }

    private Result(Error error) : base(false, error)
    {
        _value = default;
    }

    public static Result<T> Success(T value) => new(value);

    public new static Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Implicit conversion from T to Result&lt;T&gt; for ergonomic usage.
    /// Allows returning a value directly from methods that return Result&lt;T&gt;.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}
