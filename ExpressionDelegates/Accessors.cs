using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressionDelegates
{
    /// <summary>
    /// A storage of all generated field/property accessors used in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
    /// </summary>
    public static class Accessors
    {
        private static readonly Dictionary<string, Accessor> _cache = new Dictionary<string, Accessor>();

        /// <summary>
        /// You probably wont need this method. It used in generated code for automatic delegate
        /// registration at the target assembly load.
        /// </summary>
        public static void Add(string signature, Func<object?, object> getter, Action<object?, object>? setter)
        {
            _cache[signature] = new Accessor(getter, setter);
        }

        /// <summary>
        /// Provides a generated accessor for the field or property used in
        /// <see cref="System.Linq.Expressions.Expression{TDelegate}"/> by its signature. <para />
        /// Please use <see cref="Find(MemberInfo)"/> overload when possible, it's safer.
        /// </summary>
        /// <param name="signature">Namespace.Class.Property e. g. <c>System.Collections.Generic.List&lt;System.String&gt;.Capacity</c></param>
        /// <returns>null if not found</returns>
        public static Accessor? Find(string signature)
        {
            _cache.TryGetValue(signature, out var accessor);
            return accessor;
        }

        /// <summary>
        /// Provides a generated accessor for <see cref="System.Linq.Expressions.MemberExpression.Member"/>
        /// visited in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>
        /// </summary>
        /// <param name="member">
        /// <see cref="PropertyInfo"/> or <see cref="FieldInfo"/> from <see cref="System.Linq.Expressions.MemberExpression.Member"/>
        /// </param>
        /// <returns>null if not found</returns>
        public static Accessor? Find(MemberInfo member)
        {
            var path = ReflectionNameBuilder.PropertyFieldSignature(member);

            return Find(path);
        }
    }
}