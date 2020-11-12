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

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.450 (2004/?/20H1)
AMD Ryzen 5 1600, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.403
  [Host]     : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT
  DefaultJob : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT


```
|                   Type |                                            Method |           Mean |         Error |      StdDev |
|----------------------- |-------------------------------------------------- |---------------:|--------------:|------------:|
| ConstructorPerformance |                           'Cached Compile Invoke' |      4.0403 ns |     0.0955 ns |   0.0893 ns |
| ConstructorPerformance |                          'Direct Delegate Invoke' |      4.5348 ns |     0.0373 ns |   0.0291 ns |
| ConstructorPerformance |   'Cached ExpressionDelegates.Constructor Invoke' |      6.0975 ns |     0.0574 ns |   0.0479 ns |
| ConstructorPerformance |                            ConstructorInfo.Invoke |    123.8007 ns |     0.6502 ns |   0.5764 ns |
| ConstructorPerformance | 'ExpressionDelegates.Constructor Find and Invoke' |    195.5568 ns |     2.9894 ns |   2.7962 ns |
| ConstructorPerformance |                    'Cached Interpretation Invoke' |    218.5232 ns |     2.4213 ns |   2.2648 ns |
| ConstructorPerformance |                            'Interpret and Invoke' |  2,159.6494 ns |    13.6870 ns |  11.4293 ns |
| ConstructorPerformance |                              'Compile and Invoke' | 87,383.7708 ns | 1,015.4136 ns | 900.1377 ns |
|      GetterPerformance |                           'Cached Compile Invoke' |      1.2099 ns |     0.0226 ns |   0.0200 ns |
|      GetterPerformance |                          'Direct Delegate Invoke' |      1.7885 ns |     0.0099 ns |   0.0083 ns |
|      GetterPerformance |                    'Cached CreateDelegate Invoke' |      3.8704 ns |     0.0106 ns |   0.0089 ns |
|      GetterPerformance |      'Cached ExpressionDelegates.Accessor Invoke' |      5.5564 ns |     0.0825 ns |   0.0689 ns |
|      GetterPerformance |                    'Cached Interpretation Invoke' |    101.4719 ns |     0.8175 ns |   0.7247 ns |
|      GetterPerformance |                             PropertyInfo.GetValue |    161.7519 ns |     1.9527 ns |   1.8265 ns |
|      GetterPerformance |    'ExpressionDelegates.Accessor Find and Invoke' |    165.8072 ns |     1.3089 ns |   1.1603 ns |
|      GetterPerformance |                       'CreateDelegate and Invoke' |    549.4610 ns |     4.9774 ns |   4.4123 ns |
|      GetterPerformance |                            'Interpret and Invoke' |  2,717.5017 ns |    21.5642 ns |  20.1712 ns |
|      GetterPerformance |                              'Compile and Invoke' | 84,844.4946 ns |   347.9795 ns | 325.5002 ns |
|      MethodPerformance |                           'Cached Compile Invoke' |      0.8923 ns |     0.0095 ns |   0.0079 ns |
|      MethodPerformance |                          'Direct Delegate Invoke' |      1.1899 ns |     0.0049 ns |   0.0039 ns |
|      MethodPerformance |                    'Cached CreateDelegate Invoke' |      2.9685 ns |     0.0063 ns |   0.0056 ns |
|      MethodPerformance |        'Cached ExpressionDelegates.Method Invoke' |      4.4956 ns |     0.0337 ns |   0.0282 ns |
|      MethodPerformance |                    'Cached Interpretation Invoke' |     90.8100 ns |     1.0490 ns |   0.9812 ns |
|      MethodPerformance |                                 MethodInfo.Invoke |    118.1071 ns |     0.9009 ns |   0.8427 ns |
|      MethodPerformance |      'ExpressionDelegates.Method Find and Invoke' |    207.5406 ns |     0.8743 ns |   0.7751 ns |
|      MethodPerformance |                       'CreateDelegate and Invoke' |    527.0017 ns |     5.6306 ns |   5.2669 ns |
|      MethodPerformance |                            'Interpret and Invoke' |  2,619.2972 ns |    37.3735 ns |  33.1306 ns |
|      MethodPerformance |                              'Compile and Invoke' | 79,586.0026 ns |   916.4395 ns | 857.2381 ns |

</details>

## Contributions

If you've found an error, please file an issue.

Patches are encouraged, and may be submitted by forking this project and submitting a pull request. If your change is substantial, please raise an issue first to discuss it.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
