using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace XperienceCommunity.CQRS.Core
{
    /// <summary>
    /// Service for dispatching <see cref="ICommand"/>, <see cref="ICommand{TResponse}"/>, and <see cref="ICommand{TResponse, TError}"/> along
    /// with their synchronous counterparts.
    /// Commands are executed by their <see cref="ICommandHandler{TCommand}"/> implementations.
    /// </summary>
    public interface ICommandDispatcher
    {
        /// <summary>
        /// Dispatches an asynchronous <see cref="ICommand"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Result> Dispatch(ICommand command, CancellationToken token = default);

        /// <summary>
        /// Dispatches an asynchronous <see cref="ICommand{TResponse}"/>
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Result<TResponse>> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken token = default);

        /// <summary>
        /// Dispatches an asynchronous <see cref="ICommand{TResponse, TError}"/>
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Result<TResponse, TError>> Dispatch<TResponse, TError>(ICommand<TResponse, TError> command, CancellationToken token = default);
    }

    /// <inheritdoc />
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly OperationServiceFactory serviceFactory;
        private static readonly ConcurrentDictionary<Type, object> commandHandlers = new();

        public CommandDispatcher(OperationServiceFactory serviceFactory) =>
            this.serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));

        /// <inheritdoc />
        public Task<Result> Dispatch(ICommand command, CancellationToken token = default)
        {
            var commandType = command.GetType();

            var handler = (CommandHandlerWrapper)commandHandlers
                .GetOrAdd(
                    commandType,
                    t => Activator
                        .CreateInstance(typeof(CommandHandlerWrapperImpl<>)
                        .MakeGenericType(commandType)) ?? throw new InvalidOperationException($"Could not create wrapper type for {t}"));
            ;

            return handler.Dispatch(command, serviceFactory);
        }

        /// <inheritdoc />
        public Task<Result<TResponse>> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken token = default)
        {
            var commandType = command.GetType();

            var handler = (CommandHandlerWrapper<TResponse>)commandHandlers
                .GetOrAdd(
                    commandType,
                    t => Activator
                        .CreateInstance(typeof(CreateCommandHandlerWrapperImpl<,>)
                        .MakeGenericType(commandType, typeof(TResponse))) ?? throw new InvalidOperationException($"Could not create wrapper type for {t}"));

            return handler.Dispatch(command, serviceFactory);
        }

        /// <inheritdoc />
        public Task<Result<TResponse, TError>> Dispatch<TResponse, TError>(ICommand<TResponse, TError> command, CancellationToken token = default)
        {
            var commandType = command.GetType();

            var handler = (CommandHandlerWrapper<TResponse, TError>)commandHandlers
                .GetOrAdd(
                    commandType,
                    t => Activator
                        .CreateInstance(typeof(CreateCommandHandlerWrapperImpl<,,>)
                        .MakeGenericType(commandType, typeof(TResponse), typeof(TError))) ?? throw new InvalidOperationException($"Could not create wrapper type for {t}"));

            return handler.Dispatch(command, serviceFactory);
        }
    }

    internal abstract class CommandHandlerWrapper : HandlerBase
    {
        public abstract Task<Result> Dispatch(ICommand command, OperationServiceFactory serviceFactory);
    }

    internal class CommandHandlerWrapperImpl<TCommand> : CommandHandlerWrapper
        where TCommand : ICommand
    {
        public override Task<Result> Dispatch(ICommand command, OperationServiceFactory serviceFactory) =>
            GetHandler<ICommandHandler<TCommand>>(serviceFactory).Execute((TCommand)command);
    }

    internal abstract class CommandHandlerWrapper<TResponse> : HandlerBase
    {
        public abstract Task<Result<TResponse>> Dispatch(ICommand<TResponse> command, OperationServiceFactory serviceFactory);
    }

    internal class CreateCommandHandlerWrapperImpl<TCommand, TResponse> : CommandHandlerWrapper<TResponse>
        where TCommand : ICommand<TResponse>
    {
        public override Task<Result<TResponse>> Dispatch(ICommand<TResponse> command, OperationServiceFactory serviceFactory) =>
            GetHandler<ICommandHandler<TCommand, TResponse>>(serviceFactory).Execute((TCommand)command);
    }

    internal abstract class CommandHandlerWrapper<TResponse, TError> : HandlerBase
    {
        public abstract Task<Result<TResponse, TError>> Dispatch(ICommand<TResponse, TError> command, OperationServiceFactory serviceFactory);
    }

    internal class CreateCommandHandlerWrapperImpl<TCommand, TResponse, TError> : CommandHandlerWrapper<TResponse, TError>
        where TCommand : ICommand<TResponse, TError>
    {
        public override Task<Result<TResponse, TError>> Dispatch(ICommand<TResponse, TError> command, OperationServiceFactory serviceFactory) =>
            GetHandler<ICommandHandler<TCommand, TResponse, TError>>(serviceFactory).Execute((TCommand)command);
    }

}
