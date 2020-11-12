using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressionDelegates
{
    /// <summary>
    /// A storage of all generated methods used in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
    /// </summary>
    public static class Methods
    {
        private static readonly Dictionary<string, Method> _cache = new Dictionary<string, Method>();

        /// <summary>
        /// You probably wont need this method. It used in generated code for automatic delegate
        /// registration at the target assembly load.
        /// </summary>
        public static void Add(string signature, Action<object?, object[]> invoke)
        {
            _cache[signature] = new Method(invoke);
        }

        /// <summary>
        /// You probably wont need this method. It used in generated code for automatic delegate
        /// registration at the target assembly load.
        /// </summary>
        public static void Add(string signature, Func<object?, object[], object> invoke)
        {
            _cache[signature] = new Method(invoke);
        }

        /// <summary>
        /// Provides a generated delegate for a method used in
        /// <see cref="System.Linq.Expressions.Expression{TDelegate}"/> by its signature. <para />
        /// Please use <see cref="Find(MethodInfo)"/> overload when possible, it's safer.
        /// </summary>
        /// <param name="signature">Namespace.Class.Method(ParameterType1, ParameterType2) e. g. <c>System.Collections.Generic.List&lt;System.String&gt;.Sort()</c></param>
        /// <returns>null if not found</returns>
        public static Method? Find(string signature)
        {
            _cache.TryGetValue(signature, out var method);
            return method;
        }

        /// <summary>
        /// Provides a generated method delegate for <see cref="System.Linq.Expressions.MethodCallExpression.Method"/>
        /// visited in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>
        /// </summary>
        /// <returns>null if not found</returns>
        public static Method? Find(MethodInfo method)
        {
            var signature = ReflectionNameBuilder.MethodSignature(method);

            return Find(signature);
        }
    }
}
