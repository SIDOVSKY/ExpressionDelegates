using System;

namespace ExpressionDelegates
{
    public class Accessor
    {
        public Accessor(Func<object?, object> getter, Action<object?, object>? setter)
        {
            Get = getter;
            Set = setter;
        }

        public Func<object?, object> Get { get; }
        public Action<object?, object>? Set { get; }
    }
}