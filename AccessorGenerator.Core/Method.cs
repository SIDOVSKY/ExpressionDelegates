using System;

namespace ExpressionDelegates.Base
{
    public class Method
    {
        private readonly Func<object, object[], object?> _invoke;

        public Method(Func<object, object[], object?> invoke)
        {
            _invoke = invoke;
        }

        public object? Invoke(object obj, params object[] args)
        {
            return _invoke(obj, args);
        }
    }
}