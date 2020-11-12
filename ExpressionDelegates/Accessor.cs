using System;

namespace ExpressionDelegates
{
    /// <summary>
    /// Getter + Setter delegates for a separate property signature.
    /// </summary>
    public class Accessor
    {
        public Accessor(Func<object?, object> getter, Action<object?, object>? setter)
        {
            Get = getter;
            Set = setter;
        }

        /// <summary>
        /// Object getter. Usage:
        /// <code>
        /// var propertyValue = accessor.Get(objectWithProperty)
        /// </code>
        /// </summary>
        public Func<object?, object> Get { get; }

        /// <summary>
        /// Object setter. Usage:
        /// <code>
        /// accessor.Set(objectWithProperty, propertyValue)
        /// </code>
        /// </summary>
        public Action<object?, object>? Set { get; }
    }
}