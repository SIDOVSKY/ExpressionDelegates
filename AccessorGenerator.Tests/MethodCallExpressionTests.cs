using AccessorGenerator.Core;
using System;
using System.Linq.Expressions;
using Xunit;

namespace AccessorGenerator.Tests
{
    public class MethodCallExpressionTests
    {
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

        public class MethodTestClass
        {
            public static void StaticVoid() { }
            public static void StaticReturnObject() => new object();

            public void ReturnVoid() { }
            public object OneParameter(object parameter) => parameter;
            public object ReturnObject() => new object();
            public T GenericReturn<T, T2>(T parameter, T2 parameter2) => parameter;
        }
    }
}