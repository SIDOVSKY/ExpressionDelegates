using System;
using System.Text;

namespace ExpressionDelegates.Base
{
    public static class ReflectionNameBuilder
    {
        public static StringBuilder FullTypeName(Type type, StringBuilder? sb = null)
        {
            sb ??= new StringBuilder();

            if (!type.IsGenericType)
            {
                sb.Append(type.FullName).Replace('+', '.');
                return sb;
            }

            sb.Append(type.FullName, 0, type.FullName.IndexOf('`')).Replace('+', '.');
            sb.Append('<');

            var appendComma = false;
            foreach (var typeParam in type.GetGenericArguments())
            {
                if (appendComma)
                {
                    sb.Append(", ");
                }

                FullTypeName(typeParam, sb);
                appendComma = true;
            }
            sb.Append('>');
            return sb;
        }
    }
}
