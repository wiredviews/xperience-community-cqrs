namespace XperienceCommunity.CQRS.Core
{
    /// <summary>
    /// Represents an asynchronous command that returns no value when executed
    /// </summary>
    public interface ICommand { }

    /// <summary>
    /// Represents an asynchronous command that returns <typeparamref name="TResponse"/> when executed
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICommand<TResponse> { }

    /// <summary>
    /// Represents an asynchronous command that returns <typeparamref name="TResponse"/> on success and <typeparamref name="TError"/> on failure when executed
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TError"></typeparam>
    public interface ICommand<TResponse, TError> { }
}
