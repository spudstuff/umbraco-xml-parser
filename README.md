Umbraco XML Parser
==================

This repository contains code for a NuGet package that allows you to easily parse the Umbraco v4/v6/v7 XML cache file `umbraco.config`.

This XML cache contains all published content and property data and can be used programmatically for any of the following purposes:

* Content migration _(eg. migrate all published content elsewhere)_
* Content analysis _(eg. how many articles do not have a populated meta description property?)_
* Content reporting  _(eg. how many published articles do we have this month?)_

As of version [1.0.2](https://www.nuget.org/packages/RecursiveMethod.UmbracoXmlParser/1.0.2) and later, Umbraco XML Parser understands the commonly used [umbracoUrlAlias](https://our.umbraco.org/Documentation/Reference/Routing/routing-properties) and [umbracoUrlName](https://our.umbraco.org/Documentation/Reference/Routing/routing-properties) elements to set the URL of the node in question. Note if the umbracoUrlAlias contains multiple aliases (comma separated), then only the first will be used as the URL.

Getting
-------

Pull the package with nuget: `install-package RecursiveMethod.UmbracoXmlParser`

Using
-----
Some sample [LINQPad](https://www.linqpad.net/) scripts.

##### Count all nodes of a certain doctype ("Articles"):
```cs
var parser = new UmbracoXmlParser("umbraco.config");
var articleCount = parser.GetNodes().Where(n => n.Doctype == "Article").Count();
articleCount.Dump();
```

##### Dump all articles or reviews that do not have a meta description populated
```cs
var parser = new UmbracoXmlParser("umbraco.config");
var articles = parser.GetNodes().Where(n => (n.Doctype == "Article" || n.Doctype == "Review") &&
                                             string.IsNullOrWhiteSpace(n.GetPropertyAsString("metaDescription")));
articles.Dump();
```

##### Dump all nodes in the site with their node ID and URL, using a specific domain for the root level node (1069 in this case):
```cs
var parser = new UmbracoXmlParser("umbraco.config", new Dictionary<int, string> { { 1069, "https://www.examplesite.com.au" } });
var articles = parser.GetNodes().Select(n => new { NodeId = n.Id, Url = n.Url });
articles.Dump();
```

Reference
---------

#### UmbracoXmlParser Constructors
| Name                                                                             | Description                                                                                                                                |
| ---------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------|
| UmbracoXmlParser(string umbracoConfig)                                           | Construct a new UmbracoXmlParser instance by parsing the supplied umbraco.config XML cache file.                                           |
| UmbracoXmlParser(string umbracoConfig, Dictionary<int, string> urlPrefixMapping) | Construct a new UmbracoXmlParser instance by parsing the supplied umbraco.config XML cache file and a mapping of node IDs to URL prefixes. |

The second constructor allows you to associate one or more URL prefixes ("domain names") to nodes. For example, if you have two sites in your content tree, each bound to a domain name, you can specify that in the constructor:

```cs
var parser = new UmbracoXmlParser("umbraco.config", new Dictionary<int, string>
{
    { 1069, "https://www.examplesite.com.au" },
    { 1071, "https://www.othersite.com.au" },
});
```

#### UmbracoXmlParser Methods
| Name                | Description                                                                                                                 |
| --------------------|-----------------------------------------------------------------------------------------------------------------------------|
| GetNode(int nodeId) | Get a specific node by node ID. Returns null if the node does not exist (or isn't published and hence not in the XML cache. |
| GetNodes()          | Returns an IEnumerable of UmbracoNode in the order that they are specified  in the umbraco.config XML cache.                |

``` ```

#### UmbracoNode Properties
| Name        | Description                                                                                    |
| ------------|------------------------------------------------------------------------------------------------|
| Id          | Node ID.                                                                                       |
| ParentId    | Parent node ID. Will be `NULL` (and not -1) for the top-level nodes.                           |
| Parent      | Parent node of type `UmbracoNode`. Will be `NULL` for the top-level nodes.                     |
| Name        | Node name.                                                                                     |
| Url         | Node URL (see also the ability to assign URL prefixes during construction of UmbracoXmlParser. |
| PathIds     | A List<int> of node IDs from the root node to this node.                                       |
| PathNames   | A List<string> of node names from the root node to this node.                                  |
| Doctype     | The document type of this node.                                                                |
| Level       | The level / depth of this node. Top level nodes have a level of 1.                             |
| CreateDate  | Date the node was created.                                                                     |
| UpdateDate  | Date the node was updated.                                                                     |
| CreatorName | Name of the creator.                                                                           |
| WriterName  | Name of the writer.                                                                            |
| TemplateId  | Internal Umbraco template ID. Can be used to identify nodes by template.                       |

#### UmbracoNode Methods
| Name                                        | Description                                                                                    |
| --------------------------------------------|------------------------------------------------------------------------------------------------|
| GetPropertyAsString(string propertyName)    | Gets the named property as a string.                                                           |
| GetPropertyAsBool(string propertyName)      | Gets the named property as a bool. Only the value 1 is treated as true.                        |
| GetPropertyAsInt(string propertyName)       | Gets the named property as an int.                                                             |
| GetPropertyAsDate(string propertyName)      | Gets the named property as a datetime. Format must be yyyy-MM-ddTHH:mm:ss.                     |
| GetPropertyAsXmlString(string propertyName) | Gets the named property as an XML formatted string.                                            |
| GetProperties()                             | Get all properties in a dictionary with property names as keys and the values cast as strings. |

