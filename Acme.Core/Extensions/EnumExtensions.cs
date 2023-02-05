using System.ComponentModel;
using System.Reflection;

namespace Acme.Core.Extensions;

public static class EnumExtensions
{
    public static string? GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());

        if (fieldInfo == null)
        {
            return null;
        }

        var attribute =
            (DescriptionAttribute) fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute))!;

        return attribute?.Description;
    }

    public static string? GetName(this Enum value)
    {
        return Enum.GetName(typeof(Enum), value);
    }

    public static int ToInt(this Enum value)
    {
        return Convert.ToInt32(value);
    }
}