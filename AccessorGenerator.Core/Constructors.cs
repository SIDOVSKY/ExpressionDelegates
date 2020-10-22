using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpressionDelegates.Base
{
    public static class Constructors
    {
        private static readonly Dictionary<string, Constructor> _cache
            = new Dictionary<string, Constructor>();

        public static void Add(string path, Func<object[], object> invoke)
        {
            _cache[path] = new Constructor(invoke);
        }

        public static Constructor? Find(string path)
        {
            _cache.TryGetValue(path, out var constructor);
            return constructor;
        }

        public static Constructor? Find(ConstructorInfo constructor)
        {
            var fullType = ReflectionNameBuilder.FullTypeName(constructor.DeclaringType).ToString();

            var genericArgs = constructor.IsGenericMethod
                ? '<' + string.Join(", ", constructor.GetGenericArguments().Select(a => ReflectionNameBuilder.FullTypeName(a).ToString())) + '>'
                : string.Empty;

            var parameters = string.Join(", ", constructor.GetParameters()
                .Select(p => ReflectionNameBuilder.FullTypeName(p.ParameterType).ToString()));

            var path = $"{fullType}.{constructor.DeclaringType.Name}{genericArgs}({parameters})";
            return Find(path);
        }
    }
}
