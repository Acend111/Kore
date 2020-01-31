using Invector;
using Invector.vItemManager;
using System;
using System.Reflection;

namespace EviLA.AddOns.RPGPack.Util
{
    // CREDITS GO TO : http://blog.lexique-du-net.com/index.php?post/2010/04/08/Simple-properties-Mapper-by-reflection

    public static class PropertiesMapper
    {
        public static T CloneAndUpcast<T>(this vItemCollection b) where T : vItemCollection, new()
        {
            var clone = new T();

            var members = b.GetType().GetMembers(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].MemberType == MemberTypes.Property)
                {
                    try
                    {
                        clone
                            .GetType()
                            .GetProperty(members[i].Name)
                            .SetValue(clone, b.GetType().GetProperty(members[i].Name).GetValue(b, null), null);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.Log(e.Message);
                    }
                }
            }

            return clone;
        }
        /// <summary>
        /// Copies all the properties of the "from" object to this object if they exists.
        /// </summary>
        /// <param name="to">The object in which the properties are copied</param>
        /// <param name="from">The object which is used as a source</param>
        /// <param name="excludedProperties">Exclude these proeprties from the copy</param>
        public static void CopyPropertiesFrom(this object to, object from, string[] excludedProperties)
        {
            var targetType = to.GetType();
            var sourceType = from.GetType();

            PropertyInfo[] sourceProps = sourceType.GetProperties();
            foreach (var propInfo in sourceProps)
            {
                // filter the properties
                if (excludedProperties != null
                  && excludedProperties.vToList<string>().Contains(propInfo.Name))
                    continue;

                // Get the matching property from the target
                var toProp =
                  (targetType == sourceType) ? propInfo : targetType.GetProperty(propInfo.Name);

                // If it exists and it's writeable
                if (toProp != null && toProp.CanWrite)
                {
                    // Copty the value from the source to the target
                    var value = propInfo.GetValue(from, null);
                    toProp.SetValue(to, value, null);
                }
            }
        }

        /// <summary>
        /// Copies all the properties of the "from" object to this object if they exists.
        /// </summary>
        /// <param name="to">The object in which the properties are copied</param>
        /// <param name="from">The object which is used as a source</param>
        public static void CopyPropertiesFrom(this object to, object from)
        {
            to.CopyPropertiesFrom(from, null);
        }

        /// <summary>
        /// Copies all the properties of this object to the "to" object
        /// </summary>
        /// <param name="to">The object in which the properties are copied</param>
        /// <param name="from">The object which is used as a source</param>
        public static void CopyPropertiesTo(this object from, object to)
        {
            to.CopyPropertiesFrom(from, null);
        }

        /// <summary>
        /// Copies all the properties of this object to the "to" object
        /// </summary>
        /// <param name="to">The object in which the properties are copied</param>
        /// <param name="from">The object which is used as a source</param>
        /// <param name="excludedProperties">Exclude these proeprties from the copy</param>
        public static void CopyPropertiesTo(this object from, object to, string[] excludedProperties)
        {
            to.CopyPropertiesFrom(from, excludedProperties);
        }
    }
}