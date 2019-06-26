using System.Collections.Generic;

namespace RecursiveMethod.UmbracoXmlParser.Domain
{
    /// <summary>
    /// Options to provide mappings for URL prefixes, doctypes (Umbraco 8 only) and users (Umbraco 8 only).
    /// </summary>
    public class UmbracoParsingOptions
    {
        /// <summary>
        /// A dictionary of node ID to URL prefix association.
        /// Associating a URL prefix to a node ID substitutes that URL instead of using the Umbraco URL name.
        /// </summary>
        public Dictionary<int, string> UrlPrefixMapping { get; set; }

        /// <summary>
        /// A dictionary of content type ID to document type name.
        /// Allows a node's .Doctype property to return a name rather than numeric Content Type ID.
        /// For Umbraco 8 NuCache use only.
        /// </summary>
        public Dictionary<int, string> DoctypeMapping { get; set; }

        /// <summary>
        /// A dictionary of user ID to name association.
        /// Allows a node's .WriterName and .CreatorName properties to return names rather than null.
        /// For Umbraco 8 NuCache use only.
        /// </summary>
        public Dictionary<int, string> UserMapping { get; set; }
    }
}
