namespace XperienceCommunity.CQRS.Core;

/// <summary>
/// Handles execution of asynchronous Commands of type <see cref="ICommand"/>
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Executes the given <typeparamref name="TCommand"/> returning an asynchronous <see cref="Result"/>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Result> Execute(TCommand command, CancellationToken token = default);
}

/// <summary>
/// Handles execution of asynchronous Commands of type <see cref="ICommand{TResponse}"/>
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the given <typeparamref name="TCommand"/> returning an asynchronous <see cref="Result{T}"/>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Result<TResponse>> Execute(TCommand command, CancellationToken token = default);
}

/// <summary>
/// Handles execution of asynchronous Commands of type <see cref="ICommand{TResponse, TError}"/>
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TError"></typeparam>
public interface ICommandHandler<TCommand, TResponse, TError> where TCommand : ICommand<TResponse, TError>
{
    /// <summary>
    /// Executes the given <typeparamref name="TCommand"/> returning an asynchronous <see cref="Result{T, E}"/>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Result<TResponse, TError>> Execute(TCommand command, CancellationToken token = default);
}
