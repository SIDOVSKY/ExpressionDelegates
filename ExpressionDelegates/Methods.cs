using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressionDelegates
{
    public static class Methods
    {
        private static readonly Dictionary<string, Method> _cache = new Dictionary<string, Method>();

        public static void Add(string signature, Action<object?, object[]> invoke)
        {
            _cache[signature] = new Method(invoke);
        }

        public static void Add(string signature, Func<object?, object[], object> invoke)
        {
            _cache[signature] = new Method(invoke);
        }

        public static Method? Find(string signature)
        {
            _cache.TryGetValue(signature, out var method);
            return method;
        }

        public static Method? Find(MethodInfo method)
        {
            var signature = ReflectionNameBuilder.MethodSignature(method);

            return Find(signature);
        }
    }
}
