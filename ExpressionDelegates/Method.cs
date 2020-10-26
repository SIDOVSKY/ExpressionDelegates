using System;

namespace ExpressionDelegates
{
    public class Method
    {
        private readonly Action<object?, object[]>? _methodWithoutReturn;
        private readonly Func<object?, object[], object>? _methodWithReturn;

        public Method(Action<object?, object[]> invoke)
        {
            _methodWithoutReturn = invoke;
        }

        public Method(Func<object?, object[], object> invoke)
        {
            _methodWithReturn = invoke;
        }

        public object? Invoke(object? obj, params object[] args)
        {
            if (_methodWithoutReturn == null)
                return _methodWithReturn!(obj, args);

            _methodWithoutReturn(obj, args);
            return null;
        }
    }
}