using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressionDelegates
{
    public static class Accessors
    {
        private static readonly Dictionary<string, Accessor> _cache = new Dictionary<string, Accessor>();

        public static void Add(string signature, Func<object?, object> getter, Action<object?, object>? setter)
        {
            _cache[signature] = new Accessor(getter, setter);
        }

        public static Accessor? Find(string signature)
        {
            _cache.TryGetValue(signature, out var accessor);
            return accessor;
        }

        public static Accessor? Find(MemberInfo member)
        {
            var path = ReflectionNameBuilder.PropertyFieldSignature(member);

            return Find(path);
        }
    }
}