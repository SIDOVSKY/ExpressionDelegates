using ExpressionDelegates.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace ExpressionDelegates.Tests
{
    public class ConstructorExpressionTests
    {
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
