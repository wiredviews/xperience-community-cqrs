using System;
using System.Collections.Generic;
using System.Linq;

// From https://github.com/jbogard/MediatR

namespace XperienceCommunity.CQRS.Core
{
    /// <summary>
    /// Factory method used to resolve all services. For multiple instances, it will resolve against <see cref="IEnumerable{T}" />.
    /// Typically this is a method on a DI container or a <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <returns>An instance of type <paramref name="serviceType" /></returns>
    public delegate object? OperationServiceFactory(Type serviceType);

    public static class OperationServiceFactoryExtensions
    {
        /// <summary>
        /// Returns an instance of <typeparamref name="T"/> from the <see cref="OperationServiceFactory"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static T? GetInstance<T>(this OperationServiceFactory factory)
            => (T?) factory(typeof(T));

        public static IEnumerable<T> GetInstances<T>(this OperationServiceFactory factory)
            => (IEnumerable<T>?) factory(typeof(IEnumerable<T>)) ?? Enumerable.Empty<T>();
    }
}