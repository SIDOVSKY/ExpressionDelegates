using BenchmarkDotNet.Attributes;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionDelegates.Benchmarks
{
    public class ConstructorPerformance
    {
        public Expression<Func<TestClass>> Expression = () => new TestClass();

        private readonly ConstructorInfo _constructorInfo;
        private readonly Func<TestClass> _directDelegate = () => new TestClass();
        private readonly Func<TestClass> _cachedCompile;
        private readonly Func<TestClass> _cachedInterpret;
        private readonly Constructor _cachedConstructor;

        public ConstructorPerformance()
        {
            _constructorInfo = ((NewExpression)Expression.Body).Constructor;

            _cachedCompile = Expression.Compile();
            _cachedInterpret = Expression.Compile(preferInterpretation: true);
            _cachedConstructor = Constructors.Find(_constructorInfo);
        }

        [Benchmark(Description = "Direct Call")]
        public void Direct()
        {
            _ = new TestClass();
        }

        [Benchmark(Description = "Direct Delegate Invoke")]
        public void DirectDelegate()
        {
            _directDelegate.Invoke();
        }

        [Benchmark(Description = "Cached Compile Invoke")]
        public void CompileCache()
        {
            _cachedCompile.Invoke();
        }

        [Benchmark(Description = "Cached ExpressionDelegates.Constructor Invoke")]
        public void ExpressionDelegatesConstructorCache()
        {
            _cachedConstructor.Invoke();
        }

        [Benchmark(Description = "ConstructorInfo.Invoke")]
        public void Reflection()
        {
            _constructorInfo.Invoke(null);
        }

        [Benchmark(Description = "ExpressionDelegates.Constructor Find and Invoke")]
        public void ExpressionDelegatesConstructor()
        {
            var ctor = Constructors.Find(_constructorInfo);

            ctor.Invoke();
        }

        [Benchmark(Description = "Cached Interpretation Invoke")]
        public void InterpretCache()
        {
            _cachedInterpret.Invoke();
        }

        [Benchmark(Description = "Interpret and Invoke")]
        public void Interpret()
        {
            Expression.Compile(preferInterpretation: true).Invoke();
        }

        [Benchmark(Description = "Compile and Invoke")]
        public void Compile()
        {
            Expression.Compile().Invoke();
        }
    }
}