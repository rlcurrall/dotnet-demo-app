using System.Text;

namespace Acme.Core.Logging
{
    //---------------------------------------------------------------- 
    // Microsoft internal class: Microsoft.Extensions.Logging.Abstractions
    // https://github.com/aspnet/Logging/blob/2d2f31968229eddb57b6ba3d34696ef366a6c71b/src/Microsoft.Extensions.Logging.Abstractions/Internal/TypeNameHelper.cs
    //---------------------------------------------------------------

    internal static class TypeNameHelper
    {
        private static readonly Dictionary<Type, string?> _builtInTypeNames = new Dictionary<Type, string?>()
        {
            {
                typeof(void),
                "void"
            },
            {
                typeof(bool),
                "bool"
            },
            {
                typeof(byte),
                "byte"
            },
            {
                typeof(char),
                "char"
            },
            {
                typeof(Decimal),
                "decimal"
            },
            {
                typeof(double),
                "double"
            },
            {
                typeof(float),
                "float"
            },
            {
                typeof(int),
                "int"
            },
            {
                typeof(long),
                "long"
            },
            {
                typeof(object),
                "object"
            },
            {
                typeof(sbyte),
                "sbyte"
            },
            {
                typeof(short),
                "short"
            },
            {
                typeof(string),
                "string"
            },
            {
                typeof(uint),
                "uint"
            },
            {
                typeof(ulong),
                "ulong"
            },
            {
                typeof(ushort),
                "ushort"
            }
        };

        public static string GetTypeDisplayName(
            Type type,
            bool fullName = true,
            bool includeGenericParameterNames = false,
            bool includeGenericParameters = true,
            char nestedTypeDelimiter = '+')
        {
            var stringBuilder = new StringBuilder();
            var builder = stringBuilder;
            var type1 = type;
            var displayNameOptions = new DisplayNameOptions(fullName,
                includeGenericParameterNames, includeGenericParameters, nestedTypeDelimiter);
            ref var local = ref displayNameOptions;
            ProcessType(builder, type1, in local);
            return stringBuilder.ToString();
        }

        private static void ProcessType(
            StringBuilder builder,
            Type type,
            in DisplayNameOptions options)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                ProcessGenericType(builder, type, genericArguments, genericArguments.Length, in options);
            }
            else if (type.IsArray)
            {
                ProcessArrayType(builder, type, in options);
            }
            else
            {
                if (_builtInTypeNames.TryGetValue(type, out var str1))
                    builder.Append(str1);
                else if (type.IsGenericParameter)
                {
                    if (!options.IncludeGenericParameterNames)
                        return;
                    builder.Append(type.Name);
                }
                else
                {
                    var str2 = options.FullName ? type.FullName : type.Name;
                    builder.Append(str2);
                    if (options.NestedTypeDelimiter == '+')
                        return;
                    builder.Replace('+', options.NestedTypeDelimiter, builder.Length - str2.Length, str2.Length);
                }
            }
        }

        private static void ProcessArrayType(
            StringBuilder builder,
            Type type,
            in DisplayNameOptions options)
        {
            var type1 = type;
            while (type1.IsArray)
                type1 = type1.GetElementType();
            ProcessType(builder, type1, in options);
            for (; type.IsArray; type = type.GetElementType())
            {
                builder.Append('[');
                builder.Append(',', type.GetArrayRank() - 1);
                builder.Append(']');
            }
        }

        private static void ProcessGenericType(
            StringBuilder builder,
            Type type,
            Type[] genericArguments,
            int length,
            in DisplayNameOptions options)
        {
            var length1 = 0;
            if (type.IsNested)
                length1 = type.DeclaringType.GetGenericArguments().Length;
            if (options.FullName)
            {
                if (type.IsNested)
                {
                    ProcessGenericType(builder, type.DeclaringType, genericArguments, length1,
                        in options);
                    builder.Append(options.NestedTypeDelimiter);
                }
                else if (!string.IsNullOrEmpty(type.Namespace))
                {
                    builder.Append(type.Namespace);
                    builder.Append('.');
                }
            }

            var count = type.Name.IndexOf('`');
            if (count <= 0)
            {
                builder.Append(type.Name);
            }
            else
            {
                builder.Append(type.Name, 0, count);
                if (!options.IncludeGenericParameters)
                    return;
                builder.Append('<');
                for (var index = length1; index < length; ++index)
                {
                    ProcessType(builder, genericArguments[index], in options);
                    if (index + 1 != length)
                    {
                        builder.Append(',');
                        if (options.IncludeGenericParameterNames || !genericArguments[index + 1].IsGenericParameter)
                            builder.Append(' ');
                    }
                }

                builder.Append('>');
            }
        }

        private readonly struct DisplayNameOptions
        {
            public DisplayNameOptions(
                bool fullName,
                bool includeGenericParameterNames,
                bool includeGenericParameters,
                char nestedTypeDelimiter)
            {
                FullName = fullName;
                IncludeGenericParameters = includeGenericParameters;
                IncludeGenericParameterNames = includeGenericParameterNames;
                NestedTypeDelimiter = nestedTypeDelimiter;
            }

            public bool FullName { get; }

            public bool IncludeGenericParameters { get; }

            public bool IncludeGenericParameterNames { get; }

            public char NestedTypeDelimiter { get; }
        }
    }
}