#region - Using Statements -

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#endregion

namespace EnumHelper
{
    /// <summary>
    /// Static class used to look up enum information.
    /// </summary>
    public static class EnumLookup
    {

        #region - Fields -

        /// <summary>
        /// A dictionary of dictionaries to do a quick lookup of synonyms.
        /// </summary>
        /// <remarks>
        ///     The first key is the hash code of the type.
        ///     The second key is the synonym.
        ///     The third key is the enum value.
        /// </remarks>
        private static Dictionary<int, Dictionary<string, object>> _synonymQuickLookup = new Dictionary<int, Dictionary<string, object>>();

        /// <summary>
        /// A dictionary of dictionaries to do a quick lookup of enum attributes.
        /// </summary>
        /// <remarks>
        ///     The first key is the hash code of the type.
        ///     The second key is the full name of the enum value (e.g. PartyType.Buyer = "Buyer").
        ///     The third key is the attribute of the enum value.
        /// </remarks>
        private static Dictionary<int, Dictionary<string, EnumDescriptionAttribute>> _attributeQuickLookup = new Dictionary<int, Dictionary<string, EnumDescriptionAttribute>>();

        #endregion

        #region - Public Methods -

        /// <summary>
        /// Gets the display text.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetDisplayText<T>(T value) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enum.");
            }

            EnumDescriptionAttribute descriptionAttribute = GetDescriptionAttribute(value);
            return descriptionAttribute.DisplayText;
        }

        /// <summary>
        /// Checks if the synonym exists for the enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="synonym">The synonym.</param>
        /// <returns></returns>
        public static bool SynonymExistsForEnum<T>(string synonym) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enum.");
            }

            Dictionary<string, object> lookup = GetSynonymLookupForEnum<T>();
            return lookup.ContainsKey(synonym);
        }

        /// <summary>
        /// Determines whether the specified value is decorated by the attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is decorated by the attribute; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasAttribute<TEnum, TAttribute>(TEnum value)
            where TEnum : struct
            where TAttribute : Attribute
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("T must be an enum.");
            }

            MemberInfo enumValue = typeof(TEnum).GetMember(value.ToString())[0];
            object[] attributes = enumValue.GetCustomAttributes(typeof(TAttribute), false);

            return attributes.Length > 0;
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static TAttribute GetAttribute<TEnum, TAttribute>(TEnum value)
            where TEnum : struct
            where TAttribute : Attribute
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("T must be an enum.");
            }

            string name = value.ToString();
            MemberInfo enumValue = typeof(TEnum).GetMember(name)[0];
            object[] attributes = enumValue.GetCustomAttributes(typeof(TAttribute), false);

            if (attributes.Length <= 0)
            {
                throw new ArgumentException(string.Format("The value '{0}' in the enum '{1}' is not decorated by an attribute of type '{2}'.",
                    name,
                    typeof(TEnum).FullName,
                    typeof(TAttribute).FullName));
            }

            TAttribute attribute = (TAttribute)attributes[0];
            return attribute;
        }

        #endregion

        #region - Parsing Methods -

        /// <summary>
        /// Parses the specified enum value and returns the synonym.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string Parse<T>(T value) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enum.");
            }

            EnumDescriptionAttribute descriptionAttribute = GetDescriptionAttribute(value);
            return descriptionAttribute.Synonym;
        }

        /// <summary>
        /// Parses the specified synonym and returns the enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="synonym">The synonym.</param>
        /// <returns></returns>
        public static T Parse<T>(string synonym) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enum.");
            }

            Dictionary<string, object> lookup = GetSynonymLookupForEnum<T>();
            return (T)lookup[synonym];
        }

        /// <summary>
        /// Tries to parse the specified enum value and return the synonym.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static bool TryParse<T>(T value, out string result) where T : struct
        {
            bool success = false;

            try
            {
                result = Parse(value);
                success = true;
            }
            catch (Exception)
            {
                result = null;
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Tries to parse the specified synonym and return the enum value;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="synonym">The synonym.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static bool TryParse<T>(string synonym, out T result) where T : struct
        {
            bool success = false;

            try
            {
                result = Parse<T>(synonym);
                success = true;
            }
            catch (Exception)
            {
                result = default(T);
                success = false;
            }

            return success;
        }

        #endregion

        #region - Lookup Value Methods -

        /// <summary>
        /// Gets the description attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static EnumDescriptionAttribute GetDescriptionAttribute<T>(T value) where T : struct
        {
            int hashCode = typeof(T).GetHashCode();
            Dictionary<string, EnumDescriptionAttribute> lookup = null;

            if (_attributeQuickLookup.ContainsKey(hashCode))
            {
                lookup = _attributeQuickLookup[hashCode];
            }
            else
            {
                lookup = BuildLookupForEnumAttributes<T>();
                _attributeQuickLookup.Add(hashCode, lookup);
            }

            return lookup[value.ToString()];
        }

        /// <summary>
        /// Gets the synonym lookup for enum, by checking the lookup dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Dictionary<string, object> GetSynonymLookupForEnum<T>() where T : struct
        {
            int hashCode = typeof(T).GetHashCode();
            Dictionary<string, object> lookup = null;

            if (_synonymQuickLookup.ContainsKey(hashCode))
            {
                lookup = _synonymQuickLookup[hashCode];
            }
            else
            {
                lookup = BuildLookupForEnumSynonyms<T>();
                _synonymQuickLookup.Add(hashCode, lookup);
            }

            return lookup;
        }

        #endregion

        #region - Builder Methods -

        /// <summary>
        /// Builds the lookup dictionary for the enum synonyms.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Dictionary<string, object> BuildLookupForEnumSynonyms<T>() where T : struct
        {
            Dictionary<string, object> lookup = new Dictionary<string, object>();

            string[] enumNames = typeof(T).GetEnumNames();
            foreach (string name in enumNames)
            {
                MemberInfo enumValue = typeof(T).GetMember(name)[0];
                object[] attributes = enumValue.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);

                if (attributes.Length <= 0)
                {
                    throw new ArgumentException(string.Format("The value '{0}' in the enum '{1}' is not decorated by an attribute of type '{2}'.",
                        name,
                        typeof(T).FullName,
                        typeof(EnumDescriptionAttribute).FullName));
                }

                EnumDescriptionAttribute attribute = (EnumDescriptionAttribute)attributes[0];

                string synonym = attribute.Synonym;
                T value = (T)Enum.Parse(typeof(T), name);

                lookup.Add(synonym, value);
            }

            return lookup;
        }

        /// <summary>
        /// Builds the lookup dictionary for the enum attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Dictionary<string, EnumDescriptionAttribute> BuildLookupForEnumAttributes<T>() where T : struct
        {
            Dictionary<string, EnumDescriptionAttribute> lookup = new Dictionary<string, EnumDescriptionAttribute>();

            string[] enumNames = typeof(T).GetEnumNames();
            foreach (string name in enumNames)
            {
                MemberInfo enumValue = typeof(T).GetMember(name)[0];
                object[] attributes = enumValue.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);

                if (attributes.Length <= 0)
                {
                    throw new ArgumentException(string.Format("The value '{0}' in the enum '{1}' is not decorated by an attribute of type '{3}'.",
                        name,
                        typeof(T).FullName,
                        typeof(EnumDescriptionAttribute).FullName),
                        "T");
                }

                EnumDescriptionAttribute attribute = (EnumDescriptionAttribute)attributes[0];

                lookup.Add(name, attribute);
            }

            return lookup;
        }

        #endregion

    }
}
