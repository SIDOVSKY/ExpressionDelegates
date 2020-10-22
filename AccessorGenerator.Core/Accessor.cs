using System;

namespace ExpressionDelegates.Base
{
    public class Accessor
    {
        public Accessor(Func<object, object>? getter, Action<object, object>? setter)
        {
            Getter = getter;
            Setter = setter;
        }

        public Func<object, object>? Getter { get; }
        public Action<object, object>? Setter { get; }
    }
}