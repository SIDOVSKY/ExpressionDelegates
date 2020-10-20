using AccessorGenerator.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
        public void InternalProperty()
        {
            var list = new List<Expression<Func<TestClass, object>>>()
            {
                s => s.InternalProperty
            };

            var foundAccessor = ExpressionAccessors.Find(
                typeof(TestClass).GetProperty(nameof(TestClass.InternalProperty),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

            Assert.NotNull(foundAccessor);
        }

        public class LowAccessibilityTestClass
        {
            protected int ProtectedProperty { get; set; }
            
            private int PrivateProperty { get; set; }

            int UnspecifiedAccessibilityProperty { get; set; }

            public void Usage()
            {
                Expression<Func<LowAccessibilityTestClass, object>> _ =
                    c => c.ProtectedProperty;
                Expression<Func<LowAccessibilityTestClass, object>> __ =
                    c => c.PrivateProperty;
                Expression<Func<LowAccessibilityTestClass, object>> ___ =
                    c => c.UnspecifiedAccessibilityProperty;
            }
        }

        [Fact]
        public void ProtectedAndLowerPropertyNotFound()
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            Assert.Null(ExpressionAccessors.Find(
                typeof(LowAccessibilityTestClass).GetProperty("ProtectedProperty", flags)));
            Assert.Null(ExpressionAccessors.Find(
                typeof(LowAccessibilityTestClass).GetProperty("PrivateProperty", flags)));
            Assert.Null(ExpressionAccessors.Find(
                typeof(LowAccessibilityTestClass).GetProperty("UnspecifiedAccessibilityProperty", flags)));
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

        [Fact]
        public void Inheritance()
        {
            var list = new List<Expression<Func<TestClassChild, object>>>()
            {
                s => s.Property
            };

            var foundAccessor = ExpressionAccessors.Find(typeof(TestClassChild).GetProperty(nameof(TestClassChild.Property)));

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

            internal int InternalProperty { get; set; }

            public IDictionary<int, ICollection<string>> NestedGenericProperty { get; set; }
        }

        public class TestClassChild : TestClass
        {
        }
    }
}