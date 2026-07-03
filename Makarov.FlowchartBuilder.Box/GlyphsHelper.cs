using System;
using System.Collections.Generic;
using System.Reflection;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Box
{

    public static partial class GlyphsHelper
    {

        public static PropertyInfo[] GetActiveProperties(object obj)
        {
            return GetActiveProperties(obj.GetType());
        }

        public static PropertyInfo[] GetActiveProperties<T>()
        {
            return GetActiveProperties(typeof (T));
        }

        public static PropertyInfo[] GetActiveProperties(Type type)
        {
            var props = new List<PropertyInfo>();

            Type activePropertyAttribType = typeof(ActivePropertyAttribute);

            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {

                object[] attributes = property.GetCustomAttributes(activePropertyAttribType, true);

                if (attributes.Length == 1) props.Add(property);

                if (attributes.Length > 1)
                    throw new TooManyActivePropertyAttributesException(type.ToString(), property.Name);
            }

            return props.ToArray();
        }
    }
}
