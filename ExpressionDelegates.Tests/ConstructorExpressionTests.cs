using System;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ExpressionDelegates.Tests
{
    public class ConstructorExpressionTests
    {
        [Fact]
        public void Sample()
        {
            Expression<Func<char, int, string>> expression = (c, i) => new string(c, i);
            ConstructorInfo ctorInfo = ((NewExpression)expression.Body).Constructor;

            Constructor stringCtor = ExpressionDelegates.Constructors.Find(ctorInfo);
            // or
            stringCtor = ExpressionDelegates.Constructors.Find("System.String.String(System.Char, System.Int32)");

            var value = stringCtor.Invoke('c', 5);
            Assert.Equal("ccccc", value);
        }

        [Fact]
        public void Paramerless()
        {
            Expression<Func<ConstructionTestClass>> expr = () => new ConstructionTestClass();

            var foundCtor = Constructors.Find(
                typeof(ConstructionTestClass).GetConstructor(Array.Empty<Type>()));

            Assert.NotNull(foundCtor);
        }

        [Fact]
        public void Anonymous_Types_Are_Not_Supported()
        {
            Expression<Func<object>> expr = () => new { Hello = "" };

            var foundCtor = Constructors.Find(((NewExpression)expr.Body).Constructor);

            Assert.Null(foundCtor);
        }

        [Fact]
        public void Ref_Parameter_Is_Not_Supported()
        {
            Expression<Func<string, ConstructionTestClass>> expr = s => new ConstructionTestClass(ref s);

            var foundCtor = Constructors.Find(
                typeof(ConstructionTestClass).GetConstructor(new[] { typeof(string).MakeByRefType() }));

            Assert.Null(foundCtor);
        }

        public class ConstructionTestClass
        {
            public ConstructionTestClass() { }
            public ConstructionTestClass(ref string param) { }
        }
    }
}
