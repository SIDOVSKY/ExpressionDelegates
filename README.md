# ExpressionDelegates

[![PLATFORM](https://img.shields.io/badge/platform-.NET%20Standard%202.0-lightgrey)]() [![NuGet](https://img.shields.io/nuget/v/ExpressionDelegates.Generation)](https://www.nuget.org/packages/ExpressionDelegates.Generation/) [![NuGet](https://img.shields.io/nuget/v/ExpressionDelegates.Base?label=nuget%20%7C%20ExpressionDelegates.Base)](https://www.nuget.org/packages/ExpressionDelegates.Base/)

The purpose of this library is to compile the most common and simplest operations in expression trees during the build to avoid their slow compilation at runtime or invocation overhead after interpretation.

## Installing

Add [NuGet package](https://www.nuget.org/packages/ExpressionDelegates.Generation) to your [.NET Standard 2.0 - compatible](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md#platform-support) project

```
PM> Install-Package ExpressionDelegates.Generation
```

If your project doesn't have linq expressions to generate delegates from and you just want to use the generated delegates from other assemblies, please install [`ExpressionDelegates.Base` NuGet package](https://www.nuget.org/packages/ExpressionDelegates.Base) to avoid any code generation attempts.

```
PM> Install-Package ExpressionDelegates.Base
```

## Usage

API is very simple:
```csharp
ExpressionDelegates.Accessors.Find(string signature).Getter(object targetObject)
ExpressionDelegates.Accessors.Find(MemberInfo member).Setter(object targetObject, object value)

ExpressionDelegates.Methods.Find(string signature).Invoke(object targetObject, params object[] args)
ExpressionDelegates.Methods.Find(MethodInfo method).Invoke(object targetObject, params object[] args)

ExpressionDelegates.Constructors.Find(string signature ).Invoke(params object[] args)
ExpressionDelegates.Constructors.Find(Constructors constructor).Invoke(params object[] args)
```

### Property/Field getter and setter delegates

```csharp
Expression<Func<string, int>> expression = s => s.Length;
MemberInfo accessorInfo = ((MemberExpression)expression.Body).Member;

Accessor lengthAccessor = ExpressionDelegates.Accessors.Find(accessorInfo);
// or
lengthAccessor = ExpressionDelegates.Accessors.Find("System.String.Length");

var value = lengthAccessor.Get("17 letters string");
// value == 17
```

If property/field is read-only, `Accessor.Set` will be `null`.

If property is write-only, `Accessor.Get` will be `null`.

### Method delegates

```csharp
Expression<Func<string, char, bool>> expression = (s, c) => s.Contains(c);
MethodInfo methodInfo = ((MethodCallExpression)expression.Body).Method;

Method containsMethod = ExpressionDelegates.Methods.Find(methodInfo);
// or
containsMethod = ExpressionDelegates.Methods.Find("System.String.Contains(System.Char)");

var value = containsMethod.Invoke("Hello", 'e');
// value == true
```

### Constructor delegates

```csharp
Expression<Func<char, int, string>> expression = (c, i) => new string(c, i);
ConstructorInfo ctorInfo = ((NewExpression)expression.Body).Constructor;

Constructor stringCtor = ExpressionDelegates.Constructors.Find(ctorInfo);
// or
stringCtor = ExpressionDelegates.Constructors.Find("System.String.String(System.Char, System.Int32)");

var value = stringCtor.Invoke('c', 5);
// value == "ccccc"
```

## How does it work?

`ExpressionDelegates.Generation` package has source code generators which run through the code of a target project right before the compilation, search for `System.Linq.Expressions.Expression<T>` lambdas, extract expressions for properties, fields, method and constructor invocations and generate additional `.cs` files with registration of their delegates.

Generated files are located in `\obj\{configuration}\{platform}\g\` and look like this:
```csharp
using static ExpressionDelegates.Accessors;

namespace ExpressionDelegates.AccessorRegistration
{
    public static class ModuleInitializer
    {
        public static void Initialize()
        {
            Add("NameSpace.TestClass.Field", o => ((NameSpace.TestClass)o).Field, (t, m) => ((NameSpace.TestClass)t).Field = (System.Int32)m);
            Add("NameSpace.TestClass.InternalProperty", o => ((NameSpace.TestClass)o).InternalProperty, (t, m) => ((NameSpace.TestClass)t).InternalProperty = (System.Int32)m);
            Add("NameSpace.TestClass.NestedGenericProperty", o => ((NameSpace.TestClass)o).NestedGenericProperty, (t, m) => ((NameSpace.TestClass)t).NestedGenericProperty = (System.Collections.Generic.IDictionary<System.Int32, System.Collections.Generic.ICollection<System.String>>)m);
            Add("NameSpace.TestClass.NestingClass.NestedProperty", o => ((NameSpace.TestClass.NestingClass)o).NestedProperty, (t, m) => ((NameSpace.TestClass.NestingClass)t).NestedProperty = (System.String)m);
            Add("NameSpace.TestClass.NestingProperty", o => ((NameSpace.TestClass)o).NestingProperty, (t, m) => ((NameSpace.TestClass)t).NestingProperty = (NameSpace.TestClass.NestingClass)m);
            Add("NameSpace.TestClass.Property", o => ((NameSpace.TestClass)o).Property, (t, m) => ((NameSpace.TestClass)t).Property = (System.Int32)m);
            Add("NameSpace.TestClass.StaticProperty", o => NameSpace.TestClass.StaticProperty, (t, m) => NameSpace.TestClass.StaticProperty = (System.Int32)m);
            Add("System.Collections.Generic.IDictionary<System.Int32, System.Collections.Generic.ICollection<System.String>>.Values", o => ((System.Collections.Generic.IDictionary<System.Int32, System.Collections.Generic.ICollection<System.String>>)o).Values, null);
            Add("System.String.Length", o => ((System.String)o).Length, null);
        }
    }
}
```
Code generation is implemented using [Uno.SourceGeneration](https://github.com/unoplatform/Uno.SourceGeneration) which provides the broadest framework support while maintaining [C# Source Generators](https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md) compatibility which target C# 9 and .NET 5. The current code generation solution is likely to be replaced with С# Source Generators in the future as .NET 5 becomes more widespread.

After the code has been compiled, [ModuleInit.Fody](https://github.com/Fody/ModuleInit) injects `ModuleInitializer.Initialize()` calls into the assembly [module initializer](https://einaregilsson.com/module-initializers-in-csharp/) (assembly constructor) so that generated delegates plug in `ExpressionDelegates` classes once the target assembly is loaded. This solution will be replaced with [C# 9 Module Initializers](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/module-initializers) when C# 9 becomes more popular. 

## Possibilities and Limitations

Parameters for the delegates, their count and type checking are up to you. `InvalidCastException` is thrown in case of using wrong parameters for a delegate signature. `IndexOutOfRangeException` is thrown for an unexpected parameter count.

- No delegates for `private` or `protected` classes and [members](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/members), only for `internal` and `public`
- No delegates for methods or constructors with `ref` parameters
- No delegates for anonymous types (not sure if needed)

<!-- -->

- Static members are supported
- Generics are supported
- Nested classes are supported
- Dynamic properties/fields are supported

## Benchmarks

[Sources](ExpressionDelegates.Benchmarks)

<details>
  <summary>Results</summary>

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.572 (2004/?/20H1)
AMD Ryzen 5 1600, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT
  DefaultJob : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT


```
|                   Type |                                            Method |           Mean |       Error |      StdDev |
|----------------------- |-------------------------------------------------- |---------------:|------------:|------------:|
| ConstructorPerformance |                       'Cached CompileFast Invoke' |      4.4837 ns |   0.1249 ns |   0.1908 ns |
| ConstructorPerformance |                          'Direct Delegate Invoke' |      4.6937 ns |   0.0443 ns |   0.0370 ns |
| ConstructorPerformance |                           'Cached Compile Invoke' |      5.1806 ns |   0.3148 ns |   0.9281 ns |
| ConstructorPerformance |   'Cached ExpressionDelegates.Constructor Invoke' |      5.8940 ns |   0.0459 ns |   0.0430 ns |
| ConstructorPerformance |                            ConstructorInfo.Invoke |    121.3839 ns |   0.4510 ns |   0.4219 ns |
| ConstructorPerformance | 'ExpressionDelegates.Constructor Find and Invoke' |    191.1785 ns |   2.0766 ns |   1.7340 ns |
| ConstructorPerformance |                    'Cached Interpretation Invoke' |    199.8162 ns |   1.7841 ns |   1.6689 ns |
| ConstructorPerformance |                            'Interpret and Invoke' |  2,211.3971 ns |  13.7784 ns |  12.2142 ns |
| ConstructorPerformance |                          'CompileFast and Invoke' | 79,809.5180 ns | 473.7356 ns | 419.9543 ns |
| ConstructorPerformance |                              'Compile and Invoke' | 88,701.7674 ns | 962.4325 ns | 853.1713 ns |
|      GetterPerformance |                           'Cached Compile Invoke' |      1.4551 ns |   0.0085 ns |   0.0080 ns |
|      GetterPerformance |                          'Direct Delegate Invoke' |      1.7740 ns |   0.0291 ns |   0.0273 ns |
|      GetterPerformance |                       'Cached CompileFast Invoke' |      2.0495 ns |   0.0089 ns |   0.0084 ns |
|      GetterPerformance |                    'Cached CreateDelegate Invoke' |      2.3477 ns |   0.0526 ns |   0.0492 ns |
|      GetterPerformance |      'Cached ExpressionDelegates.Accessor Invoke' |      5.8792 ns |   0.1525 ns |   0.3009 ns |
|      GetterPerformance |                    'Cached Interpretation Invoke' |    100.4755 ns |   1.8990 ns |   3.3754 ns |
|      GetterPerformance |                             PropertyInfo.GetValue |    155.8979 ns |   1.7932 ns |   1.6774 ns |
|      GetterPerformance |    'ExpressionDelegates.Accessor Find and Invoke' |    163.2990 ns |   1.4388 ns |   1.3458 ns |
|      GetterPerformance |                       'CreateDelegate and Invoke' |    560.0708 ns |   5.8383 ns |   5.1755 ns |
|      GetterPerformance |                            'Interpret and Invoke' |  2,715.0402 ns |  19.6335 ns |  18.3652 ns |
|      GetterPerformance |                          'CompileFast and Invoke' | 74,540.2230 ns | 213.9485 ns | 200.1275 ns |
|      GetterPerformance |                              'Compile and Invoke' | 88,103.7519 ns | 235.3721 ns | 208.6513 ns |
|      MethodPerformance |                           'Cached Compile Invoke' |      0.5895 ns |   0.0132 ns |   0.0123 ns |
|      MethodPerformance |                       'Cached CompileFast Invoke' |      1.1551 ns |   0.0309 ns |   0.0258 ns |
|      MethodPerformance |                          'Direct Delegate Invoke' |      1.1767 ns |   0.0289 ns |   0.0270 ns |
|      MethodPerformance |                    'Cached CreateDelegate Invoke' |      2.0462 ns |   0.0115 ns |   0.0102 ns |
|      MethodPerformance |        'Cached ExpressionDelegates.Method Invoke' |      4.1000 ns |   0.0185 ns |   0.0173 ns |
|      MethodPerformance |                    'Cached Interpretation Invoke' |     90.7167 ns |   0.8028 ns |   0.6704 ns |
|      MethodPerformance |                                 MethodInfo.Invoke |    101.6796 ns |   0.5752 ns |   0.5381 ns |
|      MethodPerformance |      'ExpressionDelegates.Method Find and Invoke' |    186.4856 ns |   2.5224 ns |   2.3595 ns |
|      MethodPerformance |                       'CreateDelegate and Invoke' |    530.2247 ns |   3.9376 ns |   3.6832 ns |
|      MethodPerformance |                            'Interpret and Invoke' |  2,599.1128 ns |  30.2115 ns |  28.2599 ns |
|      MethodPerformance |                          'CompileFast and Invoke' | 58,216.1233 ns | 168.4641 ns | 149.3391 ns |
|      MethodPerformance |                              'Compile and Invoke' | 83,292.3139 ns | 922.4315 ns | 817.7115 ns |

</details>

## Contributions

If you've found an error, please file an issue.

Patches are encouraged, and may be submitted by forking this project and submitting a pull request. If your change is substantial, please raise an issue first to discuss it.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
