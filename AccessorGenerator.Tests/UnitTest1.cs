using AccessorGenerator.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace AccessorGenerator.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var list = new List<Expression<Func<TestClass, int>>>()
            {
                s => s.Field
            };

            var foundAccessor = ExpressionAccessors.Find(typeof(TestClass).GetField(nameof(TestClass.Field)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void Test2()
        {
            var list = new List<Expression<Func<TestClass, object>>>()
            {
                s => s.Field == s.Property
            };

            var foundAccessor = ExpressionAccessors.Find(typeof(TestClass).GetProperty(nameof(TestClass.Property)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void GenericTarget()
        {
            var list = new List<Expression<Func<
                IDictionary<int, ICollection<string>>,
                ICollection<ICollection<string>>>>>()
            {
                s => s.Values
            };

            var foundAccessor = ExpressionAccessors.Find(
                typeof(IDictionary<int, ICollection<string>>)
                    .GetProperty(nameof(IDictionary<int, ICollection<string>>.Values)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void GenericMember()
        {
            var list = new List<Expression<Func<TestClass, IDictionary<int, ICollection<string>>>>>()
            {
                s => s.NestedGenericProperty
            };

            var foundAccessor = ExpressionAccessors.Find(
                typeof(TestClass).GetProperty(nameof(TestClass.NestedGenericProperty)));

            Assert.NotNull(foundAccessor);
        }

        public class TestClass
        {
            public const int ConstField = 0;
            public readonly int ReadOnlyField = 0;
            public int Field;

            public int Property { get; set; }

            public int ReadOnlyProperty { get; }

            public int WriteOnlyProperty { get; }

            public IDictionary<int, ICollection<string>> NestedGenericProperty { get; set; }
        }
    }
}