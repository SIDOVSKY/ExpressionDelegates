using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExpressionDelegates
{
    /// <summary>
    /// A storage of all generated class constructors used in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>.
    /// </summary>
    public static class Constructors
    {
        private static readonly Dictionary<string, Constructor> _cache
            = new Dictionary<string, Constructor>();

        /// <summary>
        /// You probably wont need this method. It used in generated code for automatic delegate
        /// registration at the target assembly load.
        /// </summary>
        public static void Add(string signature, Func<object[], object> invoke)
        {
            _cache[signature] = new Constructor(invoke);
        }

        /// <summary>
        /// Provides a generated delegate for a constructor used in
        /// <see cref="System.Linq.Expressions.Expression{TDelegate}"/> by its signature. <para />
        /// Please use <see cref="Find(ConstructorInfo)"/> overload when possible, it's safer.
        /// </summary>
        /// <param name="signature">Namespace.Class.Class(ParameterType1, ParameterType2) e. g. <c>System.Collections.Generic.List&lt;System.String&gt;.List(System.Int32)</c></param>
        /// <returns>null if not found</returns>
        public static Constructor? Find(string signature)
        {
            _cache.TryGetValue(signature, out var constructor);
            return constructor;
        }

        /// <summary>
        /// Provides a generated constructor delegate for <see cref="System.Linq.Expressions.NewExpression.Constructor"/>
        /// visited in <see cref="System.Linq.Expressions.Expression{TDelegate}"/>
        /// </summary>
        /// <returns>null if not found</returns>
        public static Constructor? Find(ConstructorInfo constructor)
        {
            var signature = ReflectionNameBuilder.ConstructorSignature(constructor);

            return Find(signature);
        }
    }
}
