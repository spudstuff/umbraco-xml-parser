using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RecursiveMethod.UmbracoXmlParser.Domain
{
    public class UmbracoNode
    {
        public int Id { get; private set; }

        public UmbracoNode Parent { get; internal set; }

        public int? ParentId { get; private set; }
        public string Name { get; private set; }
        public string Url { get; private set; }
        public List<int> PathIds { get; private set; }
        public List<string> PathNames { get; private set; }
        public string Doctype { get; private set; }
        public int Level { get; private set; }
        public DateTime CreateDate { get; private set; }
        public DateTime UpdateDate { get; private set; }
        public string CreatorName { get; private set; }
        public string WriterName { get; private set; }
        public int TemplateId { get; private set; }
        private readonly XElement _element;

        internal UmbracoNode(int id, XElement element, string url, List<int> pathIds, List<string> pathNames)
        {
            Id = id;
            _element = element;

            ParentId = pathIds.Skip(pathIds.Count - 2).FirstOrDefault();
            if (ParentId == -1)
            {
                ParentId = null;
            }

            Name = pathNames.Last();
            Url = url;
            PathIds = pathIds.Skip(1).ToList(); // skip -1 root
            PathNames = pathNames;
            Doctype = element.Name.LocalName;
            Level = pathIds.Count - 1;
            try
            {
                CreateDate = DateTime.ParseExact(element.Attribute("createDate").Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.GetCultureInfo("en-us"));
            }
            catch (Exception e)
            {
                throw new UmbracoXmlParsingException(string.Format("Unparsable createDate attribute '{0}' on node ID {1}", element.Attribute("createDate").Value, Id), e);
            }
            try
            {
                UpdateDate = DateTime.ParseExact(element.Attribute("updateDate").Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.GetCultureInfo("en-us"));
            }
            catch (Exception e)
            {
                throw new UmbracoXmlParsingException(string.Format("Unparsable updateDate attribute '{0}' on node ID {1}", element.Attribute("updateDate").Value, Id), e);
            }
            CreatorName = element.Attribute("creatorName").Value;
            WriterName = element.Attribute("writerName").Value;
            TemplateId = Convert.ToInt32(element.Attribute("template").Value);
        }

        /// <summary>
        /// Gets the named property as a string.
        /// </summary>
        /// <param name="propertyName">Case sensitive property name.</param>
        /// <returns>Property value as a string, or null if not found.</returns>
        public string GetPropertyAsString(string propertyName)
        {
            if (_element.Element(propertyName) != null)
            {
                return _element.Element(propertyName).Value;
            }
            return null;
        }

        /// <summary>
        /// Gets the named property as a bool. Only the value 1 is treated as true.
        /// </summary>
        /// <param name="propertyName">Case sensitive property name.</param>
        /// <returns>Property value as a bool, or null if not found.</returns>
        public bool GetPropertyAsBool(string propertyName)
        {
            var val = GetPropertyAsString(propertyName);
            return val == "1";
        }

        /// <summary>
        /// Gets the named property as an int.
        /// </summary>
        /// <param name="propertyName">Case sensitive property name.</param>
        /// <returns>Property value as an int, or null if not found.</returns>
        public int? GetPropertyAsInt(string propertyName)
        {
            var val = GetPropertyAsString(propertyName);
            if (String.IsNullOrWhiteSpace(val))
            {
                return null;
            }
            return Convert.ToInt32(val);
        }

        /// <summary>
        /// Gets the named property as a datetime. Format must be yyyy-MM-ddTHH:mm:ss.
        /// </summary>
        /// <param name="propertyName">Case sensitive property name.</param>
        /// <returns>Property value as a datetime, or null if not found.</returns>
        public DateTime? GetPropertyAsDate(string propertyName)
        {
            var val = GetPropertyAsString(propertyName);
            if (String.IsNullOrWhiteSpace(val))
            {
                return null;
            }
            return DateTime.ParseExact(val, "yyyy-MM-ddTHH:mm:ss", CultureInfo.GetCultureInfo("en-us"));
        }

        /// <summary>
        /// Gets the named property as an XML formatted string.
        /// </summary>
        /// <param name="propertyName">Case sensitive property name.</param>
        /// <returns>Property value as an XML formatted string, or null if not found.</returns>
        public string GetPropertyAsXmlString(string propertyName)
        {
            if (_element.Element(propertyName) != null)
            {
                var xml = _element.Element(propertyName).ToString();
                var xmlDoc = XDocument.Parse(xml);
                if (xmlDoc.Root.HasElements)
                {
                    var inner = xmlDoc.Root.FirstNode.ToString();
                    return inner;
                }
            }
            return null;
        }
    }
}
