using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using RecursiveMethod.UmbracoXmlParser.Umbraco8Core;

namespace RecursiveMethod.UmbracoXmlParser.Domain
{
    public class UmbracoNode
    {
        public int Id { get; private set; }

        /// <summary>
        /// Umbraco 8.0.1 and later only.
        /// </summary>
        public string Uid { get; private set; }

        /// <summary>
        /// Get the parent of the current node.
        /// </summary>
        public UmbracoNode Parent { get; internal set; }

        /// <summary>
        /// Get children of the current node.
        /// </summary>
        public IEnumerable<UmbracoNode> Children
        {
            get
            {
                return _parser.GetNodes().Where(n => n.ParentId == Id);
            }
        }

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
        private readonly IDictionary<string, PropertyData[]> _propertyData;
        private readonly UmbracoXmlParser _parser;

        /// <summary>
        /// Construct from an Umbraco 4 through 7 XML element.
        /// </summary>
        internal UmbracoNode(UmbracoXmlParser parser, int id, XElement element, string url, List<int> pathIds, List<string> pathNames)
        {
            _element = element;
            _parser = parser;

            Id = id;

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
        /// Construct from an Umbraco 8 ContentNodeKit.
        /// </summary>
        internal UmbracoNode(UmbracoXmlParser parser, int id, ContentNodeKit treeNode, string url, List<int> pathIds, List<string> pathNames)
        {
            _parser = parser;
            if (treeNode.PublishedData != null)
            {
                _propertyData = treeNode.PublishedData.Properties;
            }

            Id = id;
            Uid = treeNode.Node.Uid.ToString().Replace("-", string.Empty).ToLower();

            ParentId = pathIds.Skip(pathIds.Count - 2).FirstOrDefault();
            if (ParentId == -1)
            {
                ParentId = null;
            }

            Name = pathNames.Last();
            Url = url;
            PathIds = pathIds.Skip(1).ToList(); // skip -1 root
            PathNames = pathNames;
            Doctype = _parser.Options.DoctypeMapping != null && _parser.Options.DoctypeMapping.ContainsKey(id) ? _parser.Options.DoctypeMapping[id] : treeNode.ContentTypeId.ToString();
            Level = pathIds.Count - 1;
            CreateDate = new DateTime(treeNode.Node.CreateDate.Ticks - (treeNode.Node.CreateDate.Ticks % TimeSpan.TicksPerSecond), treeNode.Node.CreateDate.Kind);
            UpdateDate = treeNode.PublishedData != null ? new DateTime(treeNode.PublishedData.VersionDate.Ticks - (treeNode.PublishedData.VersionDate.Ticks % TimeSpan.TicksPerSecond), treeNode.PublishedData.VersionDate.Kind) : CreateDate;
            CreatorName = _parser.Options.UserMapping != null && _parser.Options.UserMapping.ContainsKey(treeNode.Node.CreatorId) ? _parser.Options.UserMapping[treeNode.Node.CreatorId] : treeNode.Node.CreatorId.ToString();
            WriterName = _parser.Options.UserMapping != null && treeNode.PublishedData != null && _parser.Options.UserMapping.ContainsKey(treeNode.PublishedData.WriterId) ? _parser.Options.UserMapping[treeNode.PublishedData.WriterId] : treeNode.PublishedData != null ? treeNode.PublishedData.WriterId.ToString() : null;
            TemplateId = treeNode.PublishedData != null && treeNode.PublishedData.TemplateId.HasValue ? treeNode.PublishedData.TemplateId.Value : default(int);
        }

        /// <summary>
        /// Gets the named property as a string.
        /// </summary>
        /// <param name="propertyName">Case sensitive property name.</param>
        /// <returns>Property value as a string, or null if not found.</returns>
        public string GetPropertyAsString(string propertyName)
        {
            if (_element != null)
            {
                if (_element.Element(propertyName) != null)
                {
                    return _element.Element(propertyName).Value;
                }
            }
            else if (_propertyData != null)
            {
                if (_propertyData.ContainsKey(propertyName))
                {
                    var property = _propertyData[propertyName].FirstOrDefault();
                    if (property != null && property.Value != null)
                    {
                        return property.Value.ToString();
                    }
                }
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
            if (_element != null)
            {
                var val = GetPropertyAsString(propertyName);
                if (String.IsNullOrWhiteSpace(val))
                {
                    return null;
                }

                return DateTime.ParseExact(val, "yyyy-MM-ddTHH:mm:ss", CultureInfo.GetCultureInfo("en-us"));
            }

            if (_propertyData != null)
            {
                if (_propertyData.ContainsKey(propertyName))
                {
                    var property = _propertyData[propertyName].FirstOrDefault();
                    if (property != null && property.Value != null)
                    {
                        return property.Value as DateTime?;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the named property as an XML formatted string.
        /// </summary>
        /// <param name="propertyName">Case sensitive property name.</param>
        /// <returns>Property value as an XML formatted string, or null if not found.</returns>
        public string GetPropertyAsXmlString(string propertyName)
        {
            if (_element != null)
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
            }
            else if (_propertyData != null)
            {
                if (_propertyData.ContainsKey(propertyName))
                {
                    var property = _propertyData[propertyName].FirstOrDefault();
                    if (property != null)
                    {
                        var xml = property.Value as string;
                        var xmlDoc = XDocument.Parse(xml);
                        if (xmlDoc.Root.HasElements)
                        {
                            var inner = xmlDoc.Root.FirstNode.ToString();
                            return inner;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get all properties in a dictionary with property names as keys
        /// and the values cast as strings.
        /// </summary>
        /// <returns>All properties for this node in a Dictionary&lt;string, string&gt;.</returns>
        public Dictionary<string, string> GetProperties()
        {
            var dict = new Dictionary<string, string>();
            if (_element != null)
            {
                foreach (var element in _element.Elements())
                {
                    // Get value, even if XML
                    var reader = element.CreateReader();
                    reader.MoveToContent();
                    dict[element.Name.LocalName] = reader.ReadInnerXml();

                    // Drop CDATA
                    if (dict[element.Name.LocalName].StartsWith("<![CDATA["))
                    {
                        dict[element.Name.LocalName] = dict[element.Name.LocalName].Substring("<![CDATA[".Length);
                        if (dict[element.Name.LocalName].EndsWith("]]>"))
                        {
                            dict[element.Name.LocalName] = dict[element.Name.LocalName].Substring(0, dict[element.Name.LocalName].Length - "]]>".Length);
                        }
                    }
                    else
                    {
                        // Unescape XML entities
                        dict[element.Name.LocalName] = WebUtility.HtmlDecode(dict[element.Name.LocalName]);
                    }
                }
            }
            else if (_propertyData != null)
            {
                foreach (var propertyName in _propertyData.Keys)
                {
                    var property = _propertyData[propertyName].FirstOrDefault();
                    if (property != null && property.Value != null)
                    {
                        dict[propertyName] = property.Value.ToString();
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Get all properties in a dictionary with property names as keys
        /// and the values typed. This typing only occurs when using with Umbraco 8 or later.
        /// In earlier versions of Umbraco these are treated as strings.
        /// </summary>
        /// <returns>All properties for this node in a Dictionary&lt;string, object&gt;.</returns>
        public Dictionary<string, object> GetTypedProperties()
        {
            if (_element != null)
            {
                return GetProperties().ToDictionary(k => k.Key, v => (object)v.Value);
            }

            var dict = new Dictionary<string, object>();
            if (_propertyData != null)
            {
                foreach (var propertyName in _propertyData.Keys)
                {
                    var property = _propertyData[propertyName].FirstOrDefault();
                    if (property != null)
                    {
                        dict[propertyName] = property.Value;
                    }
                }
            }

            return dict;
        }
    }
}
