using System;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ExpressionDelegates.Tests
{
    public class MethodGenerationTests
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
        public void Parameterless_Method_Should_Be_Found()
        {
            Expression<Action<MethodTestClass>> expr = o => o.ReturnObject();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.ReturnObject)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Method_With_One_Parameter_Should_Be_Found()
        {
            Expression<Action<MethodTestClass>> expr = o => o.OneParameter(1);

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.OneParameter)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Wrong_Argument_Count_Should_Throw()
        {
            Expression<Action<MethodTestClass>> expr = o => o.OneParameter(1);

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.OneParameter)));

            Assert.Throws<IndexOutOfRangeException>(
                () => foundMethod.Invoke(new MethodTestClass()));
        }

        [Fact]
        public void Method_Which_Returns_Void_Should_Be_Found()
        {
            Expression<Action<MethodTestClass>> expr = o => o.ReturnVoid();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.ReturnVoid)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Generic_Method_Should_Be_Found()
        {
            Expression<Action<MethodTestClass>> expr = o => o.GenericReturn(1, "");

            var foundMethod = Methods.Find(
                typeof(MethodTestClass)
                    .GetMethod(nameof(MethodTestClass.GenericReturn))
                    .MakeGenericMethod(typeof(int), typeof(string)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Static_Method_Which_Returns_Void_Should_Be_Found()
        {
            Expression<Action> expr = () => MethodTestClass.StaticVoid();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.StaticVoid)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Static_Method_Which_Returns_Object_Should_Be_Found()
        {
            Expression<Action> expr = () => MethodTestClass.StaticReturnObject();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass).GetMethod(nameof(MethodTestClass.StaticReturnObject)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Delegate_Invoke_Should_Be_Found()
        {
            Expression<Func<MethodTestClass.Delegate, object>> expr = d => d();

            var foundMethod = Methods.Find(
                typeof(MethodTestClass.Delegate).GetMethod(nameof(MethodTestClass.Delegate.Invoke)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Raise_Event_Should_Be_Found()
        {
            var foundMethod = Methods.Find(
                typeof(MethodTestClass.EventDelegate).GetMethod(nameof(MethodTestClass.EventDelegate.Invoke)));

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Extension_Method_Should_Be_Found()
        {
            Expression<Action<MethodTestClass>> expr = o => o.Extend();

            var foundMethod = Methods.Find(((MethodCallExpression)expr.Body).Method);

            Assert.NotNull(foundMethod);
        }

        [Fact]
        public void Ref_Parameter_Is_Not_Supported()
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
        public static void Extend(this MethodGenerationTests.MethodTestClass self) { }
    }
}