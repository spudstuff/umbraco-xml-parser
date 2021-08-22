using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using RecursiveMethod.UmbracoXmlParser.Domain;
using RecursiveMethod.UmbracoXmlParser.Umbraco8Core;

namespace RecursiveMethod.UmbracoXmlParser
{
    public class UmbracoXmlParser
    {
        public UmbracoParsingOptions Options = new UmbracoParsingOptions();

        internal XDocument ParsedXml { get; private set; }
        internal BPlusTree<int, ContentNodeKit> ParsedTree { get; private set; }

        private readonly Dictionary<int, UmbracoNode> _nodes = new Dictionary<int, UmbracoNode>();
        private readonly List<UmbracoNode> _nodesInOrder = new List<UmbracoNode>();
        private readonly Dictionary<int, string> _urlFragmentCache = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _pathFragmentCache = new Dictionary<int, string>();
        private readonly Dictionary<string, int> _guidToNodeIdMapping = new Dictionary<string, int>();

        /// <summary>
        /// Construct a new <see cref="UmbracoXmlParser"/> instance by parsing the supplied
        /// umbraco.config XML cache file or NuCache database file.
        /// </summary>
        /// <param name="umbracoConfigOrNuCacheDb">Full path to umbraco.config XML cache file or NuCache database file.</param>
        public UmbracoXmlParser(string umbracoConfigOrNuCacheDb)
             : this(umbracoConfigOrNuCacheDb, (UmbracoParsingOptions)null)
        {
        }

        /// <summary>
        /// Construct a new <see cref="UmbracoXmlParser"/> instance by parsing the supplied
        /// umbraco.config XML cache file or NuCache database file and a mapping of node IDs to URL prefixes.
        /// </summary>
        /// <param name="umbracoConfigOrNuCacheDb">Full path to umbraco.config XML cache file or NuCache database file.</param>
        /// <param name="urlPrefixMapping">A dictionary of node ID to URL prefix association.
        /// Associating a URL prefix to a node ID substitutes that URL instead of using the Umbraco URL name.</param>
        [Obsolete("Use the constructor with UmbracoParsingOptions instead.")]
        public UmbracoXmlParser(string umbracoConfigOrNuCacheDb, Dictionary<int, string> urlPrefixMapping)
             : this(umbracoConfigOrNuCacheDb, new UmbracoParsingOptions { UrlPrefixMapping = urlPrefixMapping})
        {
        }

        /// <summary>
        /// Construct a new <see cref="UmbracoXmlParser"/> instance by parsing the supplied
        /// umbraco.config XML cache file or NuCache database file.
        /// </summary>
        /// <param name="umbracoConfigOrNuCacheDb">Full path to umbraco.config XML cache file or NuCache database file.</param>
        /// <param name="options">Options to provide mappings for URL prefixes, doctypes (Umbraco 8 only) and users (Umbraco 8 only).</param>
        public UmbracoXmlParser(string umbracoConfigOrNuCacheDb, UmbracoParsingOptions options)
        {
            // Save options
            if (options != null)
            {
                Options = options;
            }

            // Remove any trailing slashes from URL prefixes as we don't want them
            if (Options.UrlPrefixMapping != null)
            {
                foreach (var key in Options.UrlPrefixMapping.Keys.ToList())
                {
                    if (Options.UrlPrefixMapping[key].EndsWith("/"))
                    {
                        Options.UrlPrefixMapping[key] = Options.UrlPrefixMapping[key].TrimEnd('/');
                    }
                }
            }

            // No file?
            if (string.IsNullOrEmpty(umbracoConfigOrNuCacheDb))
            {
                throw new ArgumentException(umbracoConfigOrNuCacheDb);
            }

            // Check first few bytes. If it's XML it will start with '<' (potentially after a BOM)
            byte[] buffer = new byte[10];
            using (var stream = new FileStream(umbracoConfigOrNuCacheDb, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Read(buffer, 0, 10);
            }

            // It's an umbraco 4 through 7 XML cache file
            if (buffer[0] == '<' ||
                buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf && buffer[3] == '<') // UTF-8 BOM
            {
                try
                {
                    // Load XML into an XDocument
                    ParsedXml = XDocument.Load(umbracoConfigOrNuCacheDb);

                    // Parse content into an in-memory dictionary of node ID and node information
                    ParseXmlIntoUmbracoNodes();

                    // Destroy
                    ParsedXml = null;
                    return;
                }
                catch (UmbracoXmlParsingException ex)
                {
                    ParsedXml = null;
                    throw new UmbracoXmlParsingException($"Could not parse {umbracoConfigOrNuCacheDb} as XML - {ex.Message}");
                }
                catch
                {
                    ParsedXml = null;
                    // Might be a NuCache file
                }
            }
            
            // Umbraco 8.0.1 or later NuCache db file
            try
            {
                var keySerializer = new PrimitiveSerializer();
                var valueSerializer = new ContentNodeKitSerializer();
                var bPlusTreeOptions = new BPlusTree<int, ContentNodeKit>.OptionsV2(keySerializer, valueSerializer)
                {
                    CreateFile = CreatePolicy.Never,
                    FileName = umbracoConfigOrNuCacheDb,
                    ReadOnly = true
                };

                // Read the file into a BPlusTreeObject
                ParsedTree = new BPlusTree<int, ContentNodeKit>(bPlusTreeOptions);
            }
            catch (Exception ex)
            {
                throw new UmbracoXmlParsingException($"Could not parse {umbracoConfigOrNuCacheDb} as a NuCache DB - {ex.Message}");
            }

            // Parse content into an in-memory dictionary of node ID and node information
            ParseTreeIntoUmbracoNodes();

            // Destroy
            ParsedTree.Dispose();
            ParsedTree = null;
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
        /// Get a specific node by UID (Umbraco 8 only), for example ec4aafcc0c254f25a8fe705bfae1d324.
        /// </summary>
        /// <param name="guid">Umbraco 8 node UID.</param>
        /// <returns><see cref="UmbracoNode"/>, or null if node not found.</returns>
        public UmbracoNode GetNode(string uid)
        {
            if (uid == null)
            {
                return null;
            }

            // Translate the UID to a node ID if we can
            uid = uid.Replace("-", string.Empty).ToLower();
            if (_guidToNodeIdMapping.ContainsKey(uid))
            {
                var nodeId = _guidToNodeIdMapping[uid];
                if (_nodes.ContainsKey(nodeId))
                {
                    return _nodes[nodeId];
                }
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
            foreach (var umbracoNode in _nodesInOrder)
            {
                yield return umbracoNode;
            }
        }

        private void ParseXmlIntoUmbracoNodes()
        {
            // Iterate through each XML element in the config.
            foreach (var element in ParsedXml.Descendants())
            {
                // Elements that have an id and urlName attribute are Umbraco nodes.
                if (element.Attribute("id") != null && element.Attribute("urlName") != null)
                {
                    List<int> nodeIdPaths = GetNodeIdPath(element);
                    int nodeId = Convert.ToInt32(element.Attribute("id").Value);

                    var urlAlias = element.Element("umbracoUrlAlias");
                    string firstUrlAlias = null;
                    if (urlAlias != null && !string.IsNullOrWhiteSpace(urlAlias.Value))
                    {
                        firstUrlAlias = urlAlias.Value.Split(',')[0];
                    }

                    var url = GetUrlForNodeIdPaths(nodeIdPaths, firstUrlAlias);
                    var pathNames = GetPathNamesForNodeIdPaths(nodeIdPaths);

                    _nodes[nodeId] = new UmbracoNode(this, nodeId, element, url, nodeIdPaths, pathNames);

                    // Set parent
                    if (_nodes[nodeId].ParentId != null && _nodes.ContainsKey(_nodes[nodeId].ParentId.Value))
                    {
                        _nodes[nodeId].Parent = _nodes[_nodes[nodeId].ParentId.Value];
                    }

                    // Add to the list in order (for multiple enumeration in GetNodes())
                    _nodesInOrder.Add(_nodes[nodeId]);
                }
            }
        }

        private void ParseTreeIntoUmbracoNodes()
        {
            foreach (var nodeId in ParsedTree.Keys)
            {
                var treeNode = ParsedTree[nodeId];
                List<int> nodeIdPaths = GetNodeIdPath(treeNode);
                string umbracoUrlAlias = null;
                if (treeNode.PublishedData != null)
                {
                    var umbracoUrlAliasProperty = treeNode.PublishedData.Properties.FirstOrDefault(p => p.Key == "umbracoUrlAlias");
                    if (umbracoUrlAliasProperty.Value != null && umbracoUrlAliasProperty.Value.FirstOrDefault() != null)
                    {
                        umbracoUrlAlias = umbracoUrlAliasProperty.Value.FirstOrDefault().Value as string;
                    }
                }

                var url = GetUrlForNodeIdPaths(nodeIdPaths, umbracoUrlAlias);
                var pathNames = GetPathNamesForNodeIdPaths(nodeIdPaths);

                _nodes[nodeId] = new UmbracoNode(this, nodeId, treeNode, url, nodeIdPaths, pathNames);

                // Set parent
                if (_nodes[nodeId].ParentId != null && _nodes.ContainsKey(_nodes[nodeId].ParentId.Value))
                {
                    _nodes[nodeId].Parent = _nodes[_nodes[nodeId].ParentId.Value];
                }

                // Add to the list in order (for multiple enumeration in GetNodes())
                _nodesInOrder.Add(_nodes[nodeId]);
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

        private List<int> GetNodeIdPath(ContentNodeKit treeNode)
        {
            var paths = new List<int>();
            if (treeNode.Node != null && !string.IsNullOrEmpty(treeNode.Node.Path))
            {
                paths.AddRange(treeNode.Node.Path.Split(',').AsEnumerable().Select(i => Convert.ToInt32(i)));
            }

            return paths;
        }

        private string GetUrlForNodeIdPaths(List<int> nodeIdPaths, string umbracoUrlAlias)
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

                // If we have a custom domain attached to this nodeId AND we have a urlAlias defined, concatenate and return here
                if (!string.IsNullOrWhiteSpace(umbracoUrlAlias))
                {
                    if (Options.UrlPrefixMapping != null && Options.UrlPrefixMapping.ContainsKey(nodeId))
                    {
                        return url + "/" + umbracoUrlAlias.TrimStart('/');
                    }
                    return umbracoUrlAlias;
                }
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
            if (ParsedXml != null)
            {
                foreach (var element in ParsedXml.Descendants())
                {
                    if (element.Attribute("id") != null && element.Attribute("urlName") != null)
                    {
                        var nodeId = Convert.ToInt32(element.Attribute("id").Value);
                        var urlName = element.Attribute("urlName").Value;

                        // Can be overridden with an umbracoUrlName element
                        if (element.Element("umbracoUrlName") != null && !string.IsNullOrWhiteSpace(element.Element("umbracoUrlName").Value))
                        {
                            urlName = element.Element("umbracoUrlName").Value;
                        }

                        // Node might have a URL prefix configured
                        if (Options.UrlPrefixMapping != null && Options.UrlPrefixMapping.ContainsKey(nodeId))
                        {
                            _urlFragmentCache[nodeId] = Options.UrlPrefixMapping[nodeId];
                        }
                        else
                        {
                            _urlFragmentCache[nodeId] = urlName;

                        }

                        _pathFragmentCache[nodeId] = element.Attribute("nodeName").Value;
                    }
                }
            }
            else if (ParsedTree != null)
            {
                foreach (var nodeId in ParsedTree.Keys)
                {
                    var treeNode = ParsedTree[nodeId];

                    // Save UID
                    _guidToNodeIdMapping[treeNode.Node.Uid.ToString().Replace("-", string.Empty).ToLower()] = nodeId;

                    ContentData dataSource = treeNode.PublishedData;
                    if (dataSource == null)
                    {
                        dataSource = treeNode.DraftData;
                    }

                    if (dataSource != null && !String.IsNullOrEmpty(dataSource.UrlSegment))
                    {
                        var urlName = dataSource.UrlSegment;

                        // Can be overridden with an umbracoUrlName element
                        var umbracoUrlNameProperty = dataSource.Properties.FirstOrDefault(p => p.Key == "umbracoUrlName");
                        if (umbracoUrlNameProperty.Value != null && umbracoUrlNameProperty.Value.FirstOrDefault() != null)
                        {
                            urlName = umbracoUrlNameProperty.Value.FirstOrDefault().Value as string;
                        }

                        // Node might have a URL prefix configured
                        if (Options.UrlPrefixMapping != null && Options.UrlPrefixMapping.ContainsKey(nodeId))
                        {
                            _urlFragmentCache[nodeId] = Options.UrlPrefixMapping[nodeId];
                        }
                        else
                        {
                            _urlFragmentCache[nodeId] = urlName;

                        }

                        _pathFragmentCache[nodeId] = dataSource.Name;
                    }
                }
            }
        }
    }
}
