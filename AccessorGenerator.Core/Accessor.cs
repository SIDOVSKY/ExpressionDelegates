using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AccessorGenerator
{

    public class Accessor
    {
        public Accessor(Func<object, object>? getter, Action<object, object>? setter)
        {
            Getter = getter;
            Setter = setter;
        }

        public Func<object, object>? Getter { get; }
        public Action<object, object>? Setter { get; }
    }
}