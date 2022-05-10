namespace XperienceCommunity.CQRS.Core;

/// <summary>
/// When applied to a <see cref="IQuery{TResponse}"/>
/// this attribute indicates to the any handler decorators that any failed <see cref="Result{T}"/> should not be logged
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SkipLoggingAttribute : Attribute
{
}
