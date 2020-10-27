using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressionDelegates
{
    public static class Constructors
    {
        private static readonly Dictionary<string, Constructor> _cache
            = new Dictionary<string, Constructor>();

        public static void Add(string signature, Func<object[], object> invoke)
        {
            _cache[signature] = new Constructor(invoke);
        }

        public static Constructor? Find(string signature)
        {
            _cache.TryGetValue(signature, out var constructor);
            return constructor;
        }

        public static Constructor? Find(ConstructorInfo constructor)
        {
            var signature = ReflectionNameBuilder.ConstructorSignature(constructor);

            return Find(signature);
        }
    }
}
