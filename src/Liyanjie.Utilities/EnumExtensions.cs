﻿using System.ComponentModel;
using System.Reflection;

namespace System
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 描述
        /// </summary>
        /// <param name="enum"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Description(this Enum @enum, char separator = ',')
        {
            var type = @enum.GetType();
            var flags = type.GetTypeInfo().GetCustomAttribute<FlagsAttribute>();

            if (flags == null)
                return _Description(type, Enum.GetName(type, @enum));
            else
            {
                var values = Enum.GetValues(type);
                if (values == null || values.Length == 0)
                    return null;

                string output = null;
                foreach (var value in values)
                {
                    if ((int)value == 0)
                    {
                        if (@enum.Equals(value))
                        {
                            output = $"{output}{separator}{_Description(type, Enum.GetName(type, value))}";
                            break;
                        }
                    }
                    else if (@enum.HasFlag((Enum)value))
                        output = $"{output}{separator}{_Description(type, Enum.GetName(type, value))}";
                }

                return output.TrimStart(separator);
            }

            string _Description(Type enumType, string enumName)
            {
                if (string.IsNullOrWhiteSpace(enumName))
                    return null;

                return enumType.GetField(enumName)?.GetCustomAttribute<DescriptionAttribute>(false)?.Description;
            }
        }
    }
}
