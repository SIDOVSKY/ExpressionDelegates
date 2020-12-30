using BenchmarkDotNet.Attributes;
using FastExpressionCompiler;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionDelegates.Benchmarks
{
    public class MethodPerformance
    {
        public Expression<Action<TestClass>> Expression = s => s.Method();

        private readonly TestClass _obj = new TestClass();
        private readonly MethodInfo _methodInfo;
        private readonly Action<TestClass> _directDelegate = s => s.Method();
        private readonly Action<TestClass> _cachedCompile;
        private readonly Action<TestClass> _cachedCompileFast;
        private readonly Action<TestClass> _cachedInterpret;
        private readonly Action<TestClass> _cachedCreateDelegate;
        private readonly Method _cachedMethod;

        public MethodPerformance()
        {
            _methodInfo = ((MethodCallExpression)Expression.Body).Method;

            _cachedCompile = Expression.Compile();
            _cachedCompileFast = Expression.CompileFast();
            _cachedInterpret = Expression.Compile(preferInterpretation: true);
            _cachedCreateDelegate = (Action<TestClass>)_methodInfo.CreateDelegate(typeof(Action<TestClass>));
            _cachedMethod = Methods.Find(_methodInfo);
        }

        [Benchmark(Description = "Cached Compile Invoke")]
        public void CompileCache()
        {
            _cachedCompile.Invoke(_obj);
        }

        [Benchmark(Description = "Cached CompileFast Invoke")]
        public void CompileFastCache()
        {
            _cachedCompileFast.Invoke(_obj);
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

        [Benchmark(Description = "Cached ExpressionDelegates.Method Invoke")]
        public void ExpressionDelegatesMethodCache()
        {
            _cachedMethod.Invoke(_obj);
        }

        [Benchmark(Description = "Cached Interpretation Invoke")]
        public void InterpretCache()
        {
            _cachedInterpret.Invoke(_obj);
        }

        [Benchmark(Description = "MethodInfo.Invoke")]
        public void Reflection()
        {
            _methodInfo.Invoke(_obj, null);
        }

        [Benchmark(Description = "ExpressionDelegates.Method Find and Invoke")]
        public void ExpressionDelegatesMethod()
        {
            var method = Methods.Find(_methodInfo);

            method.Invoke(_obj);
        }

        [Benchmark(Description = "CreateDelegate and Invoke")]
        public void CreateDelegate()
        {
            var delegatee = (Action<TestClass>)_methodInfo.CreateDelegate(typeof(Action<TestClass>));
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

        [Benchmark(Description = "CompileFast and Invoke")]
        public void CompileFast()
        {
            Expression.CompileFast().Invoke(_obj);
        }
    }
}