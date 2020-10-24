using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpressionDelegates
{
    public static class Methods
    {
        private static readonly Dictionary<string, Method> _cache = new Dictionary<string, Method>();

        public static void Add(string path, Action<object, object[]> invoke)
        {
            _cache[path] = new Method(invoke);
        }

        public static void Add(string path, Func<object, object[], object> invoke)
        {
            _cache[path] = new Method(invoke);
        }

        public static Method? Find(string path)
        {
            _cache.TryGetValue(path, out var method);
            return method;
        }

        public static Method? Find(MethodInfo method)
        {
            var fullType = ReflectionNameBuilder.FullTypeName(method.DeclaringType).ToString();

            var genericArgs = method.IsGenericMethod
                ? '<' + string.Join(", ", method.GetGenericArguments().Select(a => ReflectionNameBuilder.FullTypeName(a).ToString())) + '>'
                : string.Empty;

            var parameters = string.Join(", ", method.GetParameters()
                .Select(p => ReflectionNameBuilder.FullTypeName(p.ParameterType).ToString()));

            var path = $"{fullType}.{method.Name}{genericArgs}({parameters})";
            return Find(path);
        }
    }
}
