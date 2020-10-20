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
            var fullType = BuildFullTypeName(member.DeclaringType).ToString();
            var path = $"{fullType}.{member.Name}".Replace('+', '.');
            return Find(path);

            static StringBuilder BuildFullTypeName(Type type, StringBuilder? sb = null)
            {
                sb ??= new StringBuilder();

                if (!type.IsGenericType)
                {
                    sb.Append(type.FullName);
                    return sb;
                }

                sb.Append(type.FullName, 0, type.FullName.IndexOf('`'));
                sb.Append('<');

                var appendComma = false;
                foreach (var typeParam in type.GetGenericArguments())
                {
                    if (appendComma)
                    {
                        sb.Append(", ");
                    }

                    BuildFullTypeName(typeParam, sb);
                    appendComma = true;
                }
                sb.Append('>');
                return sb;
            };
        }
    }
}