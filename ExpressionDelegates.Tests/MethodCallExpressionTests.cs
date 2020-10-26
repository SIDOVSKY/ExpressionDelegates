using System;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ExpressionDelegates.Tests
{
    public class MethodCallExpressionTests
    {
        [Fact]
        public void Sample()
        {
            Expression<Func<string, char, bool>> expression = (s, c) => s.Contains(c);
            MethodInfo methodInfo = ((MethodCallExpression)expression.Body).Method;

            Method containsMethod = ExpressionDelegates.Methods.Find(methodInfo);
            // or
            containsMethod = ExpressionDelegates.Methods.Find("System.String.Contains(System.Char)");

            var value = containsMethod.Invoke("Hello", 'e');
            Assert.Equal(true, value);
        }

        [Fact]
        public void Parameterless()
        {
            Expression<Action<MethodTestClass>> expr = o => o.ReturnObject();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.ReturnObject)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void OneParameter()
        {
            Expression<Action<MethodTestClass>> expr = o => o.OneParameter(1);

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.OneParameter)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void ReturnsVoid()
        {
            Expression<Action<MethodTestClass>> expr = o => o.ReturnVoid();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.ReturnVoid)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Generic()
        {
            Expression<Action<MethodTestClass>> expr = o => o.GenericReturn(1, "");

            var foundMethod = Methods.Find(
                typeof(MethodTestClass)
                    .GetMethod(nameof(MethodTestClass.GenericReturn))
                    .MakeGenericMethod(typeof(int), typeof(string)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void StaticReturnsVoid()
        {
            Expression<Action> expr = () => MethodTestClass.StaticVoid();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.StaticVoid)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void StaticReturnsObject()
        {
            Expression<Action> expr = () => MethodTestClass.StaticReturnObject();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.StaticReturnObject)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void DelegateInvoke()
        {
            Expression<Func<MethodTestClass.Delegate, object>> expr = d => d();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass.Delegate).GetMethod(nameof(MethodTestClass.Delegate.Invoke)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void RaiseEvent()
        {
            var foundMethod = Methods.Find(
                typeof(MethodTestClass.EventDelegate).GetMethod(nameof(MethodTestClass.EventDelegate.Invoke)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Extension()
        {
            Expression<Action<MethodTestClass>> expr = o => o.Extend();

            var foundMethod = Methods.Find(((MethodCallExpression)expr.Body).Method);

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void RefParameter_Is_Not_Supported()
        {
            Expression<Action<MethodTestClass, string>> expr = (o, a) => o.RefParameter(ref a);

            var foundMethod = Methods.Find(typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.RefParameter)));

            Assert.Null(foundMethod);
        }

        public class MethodTestClass
        {
            public delegate object Delegate();
            public delegate object EventDelegate();

            public static void StaticVoid() { }
            public static void StaticReturnObject() => new object();

            public event EventDelegate Event;

            protected void RaiseEventExpression()
            {
                Expression<Func<MethodTestClass, object>> expr = a => a.Event();
            }

            public void ReturnVoid() { }
            public object OneParameter(object parameter) => parameter;
            public object ReturnObject() => new object();
            public T GenericReturn<T, T2>(T parameter, T2 parameter2) => parameter;
            public string RefParameter(ref string parameter) => parameter;
        }
    }

    public static class MethodTestClassExtensions
    {
        public static void Extend(this MethodCallExpressionTests.MethodTestClass self) { }
    }
}