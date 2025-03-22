/*
 * This is an implementation of the Result<T> Design Pattern
 * The patter aims at reducing performance overhead caused by exception handling
 *
 * Instead of throwing an exception when things go wrong, an error prone function will return a result instead
 *
 * The result will either be a success and have the desired value
 * or the result will be an error that will be represented by a string
 *
 * This implementation uses two different private constructors.
 * One for creating a successful result and one for creating a false result
 *
 * To enhance readability the constructors are private and instead are exposed through an API: Success, and Failure
 * These two static methods call the respective constructors.
 * This is better than using the constructor directly as it is more readable.
 */


namespace FinalLab.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }

        private Result(T value)
        {
            IsSuccess = true;
            Value = value;
            Error = string.Empty;
        }

        private Result(string error)
        {
            IsSuccess = false;
            Value = default;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(value);
        public static Result<T> Failure(string error) => new Result<T>(error);
    }
}