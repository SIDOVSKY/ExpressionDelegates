using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AccessorGenerator.Core
{
    public static class ExpressionAccessors
    {
        private static readonly Dictionary<string, Accessor> _cache = new Dictionary<string, Accessor>();

        public static void Add(string path, Func<object, object> getter, Action<object, object> setter)
        {
            _cache[path] = new Accessor(getter, setter);
        }

        public static Accessor? Find(string path)
        {
            _cache.TryGetValue(path, out var accessor);
            return accessor;
        }

        public static Accessor? Find(MemberInfo member)
        {
            var fullType = ReflectionNameBuilder.FullTypeName(member.DeclaringType).ToString();
            var path = $"{fullType}.{member.Name}";
            return Find(path);
        }
    }
}