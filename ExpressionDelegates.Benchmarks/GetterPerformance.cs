using BenchmarkDotNet.Attributes;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionDelegates.Benchmarks
{
    public class GetterPerformance
    {
        public Expression<Func<TestClass, int>> Expression = s => s.Property;

        private readonly TestClass _obj = new TestClass();
        private readonly PropertyInfo _propertyInfo;
        private readonly Func<TestClass, int> _directDelegate = s => s.Property;
        private readonly Func<TestClass, int> _cachedCompile;
        private readonly Func<TestClass, int> _cachedInterpret;
        private readonly Func<TestClass, int> _cachedCreateDelegate;
        private readonly Accessor _cachedAccessor;

        public GetterPerformance()
        {
            _propertyInfo = (PropertyInfo)((MemberExpression)Expression.Body).Member;

            _cachedCompile = Expression.Compile();
            _cachedInterpret = Expression.Compile(preferInterpretation: true);
            _cachedCreateDelegate = (Func<TestClass, int>)Delegate.CreateDelegate(
                typeof(Func<TestClass, int>), _propertyInfo.GetMethod);
            _cachedAccessor = Accessors.Find(_propertyInfo);
        }

        [Benchmark(Description = "Cached Compile Invoke")]
        public void CompileCache()
        {
            _cachedCompile.Invoke(_obj);
        }

        [Benchmark(Description = "Direct Delegate Invoke")]
        public void DirectDelegate()
        {
            _directDelegate.Invoke(_obj);
        }

        [Benchmark(Description = "Cached CreateDelegate Invoke")]
        public void CreateDelegateCache()
        {
            _cachedCreateDelegate.Invoke(_obj);
        }

        [Benchmark(Description = "Cached ExpressionDelegates.Accessor Invoke")]
        public void ExpressionDelegatesAccessorCache()
        {
            _cachedAccessor.Get(_obj);
        }

        [Benchmark(Description = "Cached Interpretation Invoke")]
        public void InterpretCache()
        {
            _cachedInterpret.Invoke(_obj);
        }

        [Benchmark(Description = "PropertyInfo.GetValue")]
        public void Reflection()
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)Expression.Body).Member;
            propertyInfo.GetValue(_obj);
        }

        [Benchmark(Description = "ExpressionDelegates.Accessor Find and Invoke")]
        public void ExpressionDelegatesAccessor()
        {
            var accessor = Accessors.Find(_propertyInfo);

            accessor.Get(_obj);
        }

        [Benchmark(Description = "CreateDelegate and Invoke")]
        public void CreateDelegate()
        {
            var delegatee = (Func<TestClass, int>)Delegate.CreateDelegate(typeof(Func<TestClass, int>), _propertyInfo.GetMethod);
            delegatee.Invoke(_obj);
        }

        [Benchmark(Description = "Interpret and Invoke")]
        public void Interpret()
        {
            Expression.Compile(preferInterpretation: true).Invoke(_obj);
        }

        [Benchmark(Description = "Compile and Invoke")]
        public void Compile()
        {
            Expression.Compile().Invoke(_obj);
        }
    }
}