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
        public void Constructor()
        {
            Expression<Func<ConstructionTestClass>> expr = () => new ConstructionTestClass();

            var foundMethod = Constructors.Find(
                typeof(ConstructionTestClass).GetConstructor(Array.Empty<Type>()));

            Assert.NotNull(foundMethod);
        }

        public class ConstructionTestClass
        {
            public ConstructionTestClass() { }
        }
    }
}
