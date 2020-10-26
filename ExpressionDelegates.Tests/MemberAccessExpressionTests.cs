using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ExpressionDelegates.Tests
{
    public class MemberAccessExpressionTests
    {
        [Fact]
        public void Sample()
        {
            Expression<Func<string, int>> expression = s => s.Length;
            MemberInfo accessorInfo = ((MemberExpression)expression.Body).Member;

            Accessor lengthAccessor = ExpressionDelegates.Accessors.Find(accessorInfo);
            // or
            lengthAccessor = ExpressionDelegates.Accessors.Find("System.String.Length");

            var value = lengthAccessor.Get("17 letters string");
            Assert.Equal(17, value);
        }

        [Fact]
        public void Field_Should_Be_Found()
        {
            Expression<Func<TestClass, int>> expr = s => s.Field;

            var foundAccessor = Accessors.Find(typeof(TestClass).GetField(nameof(TestClass.Field)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void Property_Should_Be_Found()
        {
            Expression<Func<TestClass, object>> expr = s => s.Field == s.Property;

            var foundAccessor = Accessors.Find(typeof(TestClass).GetProperty(nameof(TestClass.Property)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void Nested_Property_Should_Be_Found()
        {
            Expression<Func<TestClass, object>> expr = s => s.NestingProperty.NestedProperty;

            var firstLevelAccessor = Accessors.Find(
                typeof(TestClass).GetProperty(nameof(TestClass.NestingProperty)));
            var secondLevelAccessor = Accessors.Find(
                typeof(TestClass.NestingClass).GetProperty(nameof(TestClass.NestingClass.NestedProperty)));

            Assert.NotNull(firstLevelAccessor);
            Assert.NotNull(secondLevelAccessor);
        }

        [Fact]
        public void Internal_Property_Should_Be_Found()
        {
            Expression<Func<TestClass, object>> expr = s => s.InternalProperty;

            var foundAccessor = Accessors.Find(
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
        public void Protected_And_Lower_Property_Not_Found()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            Assert.Null(Accessors.Find(
                typeof(LowAccessibilityTestClass).GetProperty("ProtectedProperty", flags)));
            Assert.Null(Accessors.Find(
                typeof(LowAccessibilityTestClass).GetProperty("PrivateProperty", flags)));
            Assert.Null(Accessors.Find(
                typeof(LowAccessibilityTestClass).GetProperty("UnspecifiedAccessibilityProperty", flags)));
        }

        [Fact]
        public void Property_Of_Generic_Class_Should_Be_Found()
        {
            Expression<Func<IDictionary<int, ICollection<string>>, ICollection<ICollection<string>>>> expr =
                s => s.Values;

            var foundAccessor = Accessors.Find(
                typeof(IDictionary<int, ICollection<string>>)
                    .GetProperty(nameof(IDictionary<int, ICollection<string>>.Values)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void Nested_Generic_Property_Should_Be_Found()
        {
            Expression<Func<TestClass, IDictionary<int, ICollection<string>>>> expr = s => s.NestedGenericProperty;

            var foundAccessor = Accessors.Find(
                typeof(TestClass).GetProperty(nameof(TestClass.NestedGenericProperty)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void Parent_Property_Found_For_Child()
        {
            Expression<Func<TestClassChild, object>> expr = s => s.Property;

            var foundAccessor = Accessors.Find(typeof(TestClassChild).GetProperty(nameof(TestClassChild.Property)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void Static_Property_Should_Be_Found()
        {
            Expression<Func<object>> expr = () => TestClass.StaticProperty;

            var foundAccessor = Accessors.Find(
                typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty),
                BindingFlags.Static | BindingFlags.Public));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void ReadOnly_Field_Has_No_Setter()
        {
            Expression<Func<TestClass, int>> expr = c => c.ReadOnlyField;
            var obj = new TestClass();
            var foundAccessor = Accessors.Find(
                typeof(TestClass).GetField(nameof(TestClass.ReadOnlyField)));

            Assert.NotNull(foundAccessor);
            Assert.Null(foundAccessor.Set);

            var value = foundAccessor.Get(obj);
            Assert.Equal(0, value);
        }

        [Fact]
        public void Const_Field_Has_No_Setter()
        {
            Expression<Func<int>> expr = () => TestClass.ConstField;
            var foundAccessor = Accessors.Find(
                typeof(TestClass).GetField(nameof(TestClass.ConstField)));

            Assert.NotNull(foundAccessor);
            Assert.Null(foundAccessor.Set);

            var value = foundAccessor.Get(null);
            Assert.Equal(0, value);
        }

        [Fact]
        public void ReadOnly_Property_Has_No_setter()
        {
            Expression<Func<TestClass, int>> expr = c => c.ReadOnlyProperty;
            var obj = new TestClass
            {
                Property = 42
            };

            var foundAccessor = Accessors.Find(
                typeof(TestClass).GetProperty(nameof(TestClass.ReadOnlyProperty)));

            Assert.NotNull(foundAccessor);
            Assert.Null(foundAccessor.Set);

            var value = foundAccessor.Get(obj);
            Assert.Equal(42, value);
        }

        [Fact]
        public void Generic_Property_Should_Be_Found()
        {
            Expression<Func<GenericTestClass<int>, int>> expr = c => c.GenericProperty;

            var foundAccessor = Accessors.Find(
                typeof(GenericTestClass<>).MakeGenericType(new[] { typeof(int)})
                    .GetProperty(nameof(GenericTestClass<int>.GenericProperty)));

            Assert.NotNull(foundAccessor);
        }

        [Fact]
        public void Dynamic_Property_Should_Be_Found()
        {
            Expression<Func<DynamicTestClass, dynamic>> expr = c => c.DynamicProperty;
            var obj = new DynamicTestClass
            {
                DynamicProperty = 42
            };

            var foundAccessor = Accessors.Find(
                typeof(DynamicTestClass).GetProperty(nameof(DynamicTestClass.DynamicProperty)));

            Assert.NotNull(foundAccessor);

            var value = foundAccessor.Get(obj);
            Assert.Equal(42, value);

            foundAccessor.Set(obj, "Hello");
            Assert.Equal("Hello", obj.DynamicProperty);
        }

        public class TestClass
        {
            public static int StaticProperty { get; set; }

            public const int ConstField = 0;
            public readonly int ReadOnlyField = 0;
            public int Field;

            public int Property { get; set; }

            public int ReadOnlyProperty => Property;

            public int WriteOnlyProperty
            {
                set => Property = value;
            }

            internal int InternalProperty { get; set; }

            public NestingClass NestingProperty { get; set; }

            public IDictionary<int, ICollection<string>> NestedGenericProperty { get; set; }

            public class NestingClass
            {
                public string NestedProperty { get; set; }
            }
        }

        public class TestClassChild : TestClass
        {
        }

        public class GenericTestClass<T>
        {
            public T GenericProperty { get; set; }
        }

        public class DynamicTestClass
        {
            public dynamic DynamicProperty { get; set; }
        }
    }
}