#region - Using Statements -

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace EnumHelper
{
    /// <summary>
    /// Attribute that contains descriptions for an enum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public sealed class EnumDescriptionAttribute:Attribute
    {

        #region - Constructors -

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="displayText">The display text.</param>
        /// <param name="synonym">The synonym.</param>
        public EnumDescriptionAttribute(string displayText, string synonym)
            : base()
        {
            this.DisplayText = displayText;
            this.Synonym = synonym;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets the display text.
        /// </summary>
        public string DisplayText { get; private set; }

        /// <summary>
        /// Gets the synonym.
        /// </summary>
        public string Synonym { get; private set; }

        #endregion

    }
}
