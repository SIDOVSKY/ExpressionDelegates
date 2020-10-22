using System;

namespace AccessorGenerator.Core
{
    public class Constructor
    {
        private readonly Func<object[], object> _invoke;

        public Constructor(Func<object[], object> invoke)
        {
            _invoke = invoke;
        }

        public object Invoke(params object[] args)
        {
            return _invoke(args);
        }
    }
}