﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RecursiveMethod.UmbracoXmlParser.Domain;

namespace RecursiveMethod.UmbracoXmlParser
{
    public class UmbracoXmlParser
    {
        public XDocument ParsedXml { get; private set; }

        private readonly Dictionary<int, UmbracoNode> _nodes = new Dictionary<int, UmbracoNode>();
        private readonly Dictionary<int, string> _urlFragmentCache = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _pathFragmentCache = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _urlPrefixMapping;

        /// <summary>
        /// Construct a new <see cref="UmbracoXmlParser"/> instance by parsing the supplied
        /// umbraco.config XML cache file.
        /// </summary>
        /// <param name="umbracoConfig">Full path to umbraco.config XML cache file.</param>
        public UmbracoXmlParser(string umbracoConfig) : this(umbracoConfig, null)
        {
        }

        /// <summary>
        /// Construct a new <see cref="UmbracoXmlParser"/> instance by parsing the supplied
        /// umbraco.config XML cache file and a mapping of node IDs to URL prefixes.
        /// </summary>
        /// <param name="umbracoConfig">Full path to umbraco.config XML cache file.</param>
        /// <param name="urlPrefixMapping">A dictionary of node ID to URL prefix association.
        /// Associating a URL prefix to a node ID substitutes that URL instead of using the urlName property in the umbraco.config.</param>
        public UmbracoXmlParser(string umbracoConfig, Dictionary<int, string> urlPrefixMapping)
        {
            _urlPrefixMapping = urlPrefixMapping;

            // Remove any trailing slashes from URL prefixes as we don't want them
            if (_urlPrefixMapping != null)
            {
                foreach (var key in _urlPrefixMapping.Keys.ToList())
                {
                    if (_urlPrefixMapping[key].EndsWith("/"))
                    {
                        _urlPrefixMapping[key] = _urlPrefixMapping[key].TrimEnd('/');
                    }
                }
            }

            // No file?
            if (string.IsNullOrEmpty(umbracoConfig))
            {
                throw new ArgumentException(umbracoConfig);
            }

            // Load XML into an XDocument
            ParsedXml = XDocument.Load(umbracoConfig);

            // Parse content into an in-memory dictionary of node ID and node information
            ParseIntoUmbracoNodes();
        }

        /// <summary>
        /// Get a specific node by node ID.
        /// </summary>
        /// <param name="nodeId">Umbraco node ID.</param>
        /// <returns><see cref="UmbracoNode"/>, or null if node not found.</returns>
        public UmbracoNode GetNode(int nodeId)
        {
            if (_nodes.ContainsKey(nodeId))
            {
                return _nodes[nodeId];
            }
            return null;
        }

        /// <summary>
        /// Returns an IEnumerable of <see cref="UmbracoNode"/> in the order that they are specified
        /// in the umbraco.config XML cache.
        /// </summary>
        /// <returns>IEnumerable of <see cref="UmbracoNode"/>.</returns>
        public IEnumerable<UmbracoNode> GetNodes()
        {
            foreach (var element in ParsedXml.Descendants())
            {
                // Elements that have an id and urlName attribute are Umbraco nodes.
                if (element.Attribute("id") != null && element.Attribute("urlName") != null)
                {
                    int nodeId = Convert.ToInt32(element.Attribute("id").Value);
                    yield return GetNode(nodeId);
                }
            }
        }

        private void ParseIntoUmbracoNodes()
        {
            // Iterate through each XML element in the config.
            foreach (var element in ParsedXml.Descendants())
            {
                // Elements that have an id and urlName attribute are Umbraco nodes.
                if (element.Attribute("id") != null && element.Attribute("urlName") != null)
                {
                    List<int> nodeIdPaths = GetNodeIdPath(element);
                    int nodeId = Convert.ToInt32(element.Attribute("id").Value);
                    var url = GetUrlForNodeIdPaths(nodeIdPaths);
                    var pathNames = GetPathNamesForNodeIdPaths(nodeIdPaths);

                    _nodes[nodeId] = new UmbracoNode(nodeId, element, url, nodeIdPaths, pathNames);
                }
            }
        }

        private List<int> GetNodeIdPath(XElement element)
        {
            var paths = new List<int>();
            paths.Add(Convert.ToInt32(element.Attribute("id").Value));
            while (element.Parent != null)
            {
                element = element.Parent;
                paths.Insert(0, Convert.ToInt32(element.Attribute("id").Value));
            }
            return paths;
        }

        private string GetUrlForNodeIdPaths(List<int> nodeIdPaths)
        {
            if (!_urlFragmentCache.Any())
            {
                BuildUrlFragmentCache();
            }

            var url = "";
            var sep = "";
            foreach (var nodeId in nodeIdPaths)
            {
                if (nodeId == -1)
                {
                    continue;
                }

                if (!_urlFragmentCache.ContainsKey(nodeId))
                {
                    return null;
                }

                var urlFragment = _urlFragmentCache[nodeId];
                url += sep + urlFragment;
                sep = "/";
            }

            return url;
        }

        private List<string> GetPathNamesForNodeIdPaths(List<int> nodeIdPaths)
        {
            if (!_pathFragmentCache.Any())
            {
                BuildUrlFragmentCache();
            }

            var pathNames = new List<string>();
            foreach (var nodeId in nodeIdPaths)
            {
                if (nodeId == -1)
                {
                    continue;
                }

                if (!_pathFragmentCache.ContainsKey(nodeId))
                {
                    return null;
                }

                var pathName = _pathFragmentCache[nodeId];
                pathNames.Add(pathName);
            }

            return pathNames;
        }

        private void BuildUrlFragmentCache()
        {
            foreach (var element in ParsedXml.Descendants())
            {
                if (element.Attribute("id") != null && element.Attribute("urlName") != null)
                {
                    var nodeId = Convert.ToInt32(element.Attribute("id").Value);

                    // Node might have a URL prefix configured
                    if (_urlPrefixMapping != null && _urlPrefixMapping.ContainsKey(nodeId))
                    {
                        _urlFragmentCache[nodeId] = _urlPrefixMapping[nodeId];
                    }
                    else
                    {
                        _urlFragmentCache[nodeId] = element.Attribute("urlName").Value;

                    }
                    _pathFragmentCache[nodeId] = element.Attribute("nodeName").Value;
                }
            }
        }
    }
}