using System;

// From https://github.com/jbogard/MediatR

namespace XperienceCommunity.CQRS.Core
{
    internal abstract class HandlerBase
    {
        protected static THandler GetHandler<THandler>(OperationServiceFactory factory)
        {
            THandler? handler;

            try
            {
                handler = factory.GetInstance<THandler>();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error constructing handler for request of type {typeof(THandler)}. Register your handlers with the container.", e);
            }

            return handler ?? throw new InvalidOperationException($"Handler was not found for request of type {typeof(THandler)}. Register your handlers with the container.");
        }
    }
}