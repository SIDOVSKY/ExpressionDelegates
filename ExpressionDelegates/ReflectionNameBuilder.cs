using System;
using System.Reflection;
using System.Text;

namespace ExpressionDelegates
{
    internal static class ReflectionNameBuilder
    {
        public static string ConstructorSignature(ConstructorInfo constructorInfo)
        {
            var ownerType = constructorInfo.DeclaringType;

            var isGenericType = ownerType.IsGenericType;
            var isGenericMethod = constructorInfo.IsGenericMethod;
            var parameters = constructorInfo.GetParameters();

            if (!isGenericType && !isGenericMethod && parameters.Length == 0)
                return ownerType.FullName.Replace('+', '.') + "." + ownerType.Name + "()";

            var sb = new StringBuilder()
                .AppendFullTypeName(type: ownerType)
                .Append('.');

            if (isGenericType)
            {
                var ownerTypeName = ownerType.Name;
                sb.Append(ownerTypeName, 0, ownerTypeName.IndexOf('`'));
            }
            else
            {
                sb.Append(ownerType.Name);
            }

            if (isGenericMethod)
            {
                sb.AppendGenericTypeParameters(constructorInfo.GetGenericArguments());
            }

            sb.AppendParameters(parameters);

            return sb.ToString();
        }

        public static string MethodSignature(MethodInfo methodInfo)
        {
            var ownerType = methodInfo.DeclaringType;

            var isGenericType = ownerType.IsGenericType;
            var isGenericMethod = methodInfo.IsGenericMethod;
            var parameters = methodInfo.GetParameters();

            if (!isGenericType && !isGenericMethod && parameters.Length == 0)
                return ownerType.FullName.Replace('+', '.') + "." + methodInfo.Name + "()";

            var sb = new StringBuilder()
                .AppendFullTypeName(ownerType)
                .Append('.')
                .Append(methodInfo.Name);

            if (isGenericMethod)
            {
                sb.AppendGenericTypeParameters(methodInfo.GetGenericArguments());
            }

            sb.AppendParameters(parameters);

            return sb.ToString();
        }

        public static string PropertyFieldSignature(MemberInfo memberInfo)
        {
            var ownerType = memberInfo.DeclaringType;

            return ownerType.IsGenericType
                ? new StringBuilder().AppendFullTypeName(ownerType).Append('.').Append(memberInfo.Name).ToString()
                : ownerType.FullName.Replace('+', '.') + "." + memberInfo.Name;
        }

        private static StringBuilder AppendFullTypeName(this StringBuilder sb, Type type)
        {
            var fullName = type.FullName.Replace('+', '.');

            if (!type.IsGenericType)
            {
                sb.Append(fullName);
                return sb;
            }

            sb.Append(fullName, 0, fullName.IndexOf('`'));
            sb.AppendGenericTypeParameters(type.GetGenericArguments());

            return sb;
        }

        private static void AppendGenericTypeParameters(this StringBuilder sb, Type[] typeParams)
        {
            sb.Append('<');

            var appendComma = false;
            for (var i = 0; i < typeParams.Length; i++)
            {
                if (appendComma)
                {
                    sb.Append(", ");
                }
                sb.AppendFullTypeName(typeParams[i]);
                appendComma = true;
            }

            sb.Append('>');
        }

        private static void AppendParameters(this StringBuilder sb, ParameterInfo[] parameters)
        {
            if (parameters.Length == 0)
            {
                sb.Append("()");
            }
            else
            {
                sb.Append('(');

                var appendComma = false;
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (appendComma)
                    {
                        sb.Append(", ");
                    }
                    sb.AppendFullTypeName(parameters[i].ParameterType);
                    appendComma = true;
                }

                sb.Append(')');
            }
        }
    }
}