using System;
using System.Collections.Generic;
using System.Reflection;

namespace AccessorGenerator.Core
{
    public static class ExpressionAccessors
    {
        private static readonly Dictionary<string, Accessor> _cache = new Dictionary<string, Accessor>();

        public static void Add(string path, Func<object, object> getter, Action<object, object> setter)
        {
            _cache[path] = new Accessor(getter, setter);
        }

        public static Accessor? Find(MemberInfo member)
        {
            var fullName = $"{member.DeclaringType.FullName}.{member.Name}".Replace('+', '.');
            _cache.TryGetValue(fullName, out var accessor);
            return accessor;
        }
    }
}