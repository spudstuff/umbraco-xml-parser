using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecursiveMethod.UmbracoXmlParser.Domain;

namespace RecursiveMethod.UmbracoXmlParser.UnitTests
{
    [TestClass]
    public class UnitTestsUmbracoConfig
    {
        private string _tempFile;

        [TestInitialize]
        public void Initialize()
        {
            _tempFile = GetUmbracoConfigFromResource();
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                File.Delete(_tempFile);
            }
            catch
            {
            }
        }

        [TestMethod]
        public void InvalidCreateDateAttribute()
        {
            bool exceptionThrown = false;
            var tempFile = GetUmbracoConfigFromResource("umbraco-bad-createdate.config");
            try
            {
                new UmbracoXmlParser(tempFile);
            }
            catch (UmbracoXmlParsingException ex)
            {
                Assert.AreEqual("Unparsable createDate attribute '2017-05-16T08:55:20.4171241+10:00' on node ID 1072", ex.Message);
                exceptionThrown = true;
            }
            try
            {
                File.Delete(_tempFile);
            }
            catch
            {
            }

            if (!exceptionThrown)
            {
                Assert.Fail("Exception not thrown while parsing invalid create date in umbraco.config");
            }
        }

        [TestMethod]
        public void InvalidUpdateDateAttribute()
        {
            bool exceptionThrown = false;
            var tempFile = GetUmbracoConfigFromResource("umbraco-bad-updatedate.config");
            try
            {
                new UmbracoXmlParser(tempFile);
            }
            catch (UmbracoXmlParsingException ex)
            {
                Assert.AreEqual("Unparsable updateDate attribute '2017-05-16T08:55:20.4171241+10:00' on node ID 1072", ex.Message);
                exceptionThrown = true;
            }
            try
            {
                File.Delete(_tempFile);
            }
            catch
            {
            }

            if (!exceptionThrown)
            {
                Assert.Fail("Exception not thrown while parsing invalid update date in umbraco.config");
            }
        }

        [TestMethod]
        public void RootNodePropertiesSet()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1069);
            Assert.AreEqual(1069, node.Id);
            Assert.IsNull(node.ParentId);
            Assert.AreEqual("SiteRoot", node.Doctype);
            Assert.AreEqual(1, node.Level);
            Assert.AreEqual("Example Site", node.Name);
            Assert.AreEqual("example-site", node.Url);
            Assert.AreEqual(new DateTime(2014, 12, 12, 13, 23, 29), node.CreateDate);
            Assert.AreEqual(new DateTime(2016, 9, 1, 16, 45, 19), node.UpdateDate);
            Assert.AreEqual("admin", node.CreatorName);
            Assert.AreEqual("james", node.WriterName);
            Assert.AreEqual(1067, node.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069 }, node.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site" }, node.PathNames);
            
            // Parent
            Assert.IsNull(node.Parent);
        }

        [TestMethod]
        public void DeepNodePropertiesSet()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2448);
            Assert.AreEqual(2448, node.Id);
            Assert.AreEqual(2447, node.ParentId);
            Assert.AreEqual("Article", node.Doctype);
            Assert.AreEqual(4, node.Level);
            Assert.AreEqual("People with No or Bad Credit Score", node.Name);
            Assert.AreEqual("example-site/news/oct-2014/people-with-bad-credit-score", node.Url);
            Assert.AreEqual(new DateTime(2015, 5, 13, 12, 10, 33), node.CreateDate);
            Assert.AreEqual(new DateTime(2015, 10, 22, 7, 42, 8), node.UpdateDate);
            Assert.AreEqual("angela", node.CreatorName);
            Assert.AreEqual("admin", node.WriterName);
            Assert.AreEqual(1067, node.TemplateId);
            CollectionAssert.AreEqual(new [] { 1069, 1239, 2447, 2448 }, node.PathIds);
            CollectionAssert.AreEqual(new [] { "Example Site", "News", "Oct 2014", "People with No or Bad Credit Score" }, node.PathNames);

            // Parent
            Assert.AreEqual(2447, node.Parent.Id);
            Assert.AreEqual(1239, node.Parent.ParentId);
            Assert.AreEqual("DateFolder", node.Parent.Doctype);
            Assert.AreEqual(3, node.Parent.Level);
            Assert.AreEqual("Oct 2014", node.Parent.Name);
            Assert.AreEqual("example-site/news/oct-2014", node.Parent.Url);
            Assert.AreEqual(new DateTime(2015, 5, 13, 12, 10, 32), node.Parent.CreateDate);
            Assert.AreEqual(new DateTime(2015, 5, 13, 12, 10, 32), node.Parent.UpdateDate);
            Assert.AreEqual("admin", node.Parent.CreatorName);
            Assert.AreEqual("admin", node.Parent.WriterName);
            Assert.AreEqual(0, node.Parent.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069, 1239, 2447 }, node.Parent.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site", "News", "Oct 2014" }, node.Parent.PathNames);
        }

        [TestMethod]
        public void RootNodeUrlPrefixSet()
        {
            var parser = new UmbracoXmlParser(_tempFile, new Dictionary<int, string> { { 1069, "https://www.example.com"} });
            var node = parser.GetNode(1069);
            Assert.AreEqual("https://www.example.com", node.Url);

            // Node 2499 has an umbracoUrlAlias set, which is used along side the root node URL prefix
            node = parser.GetNode(2499);
            Assert.AreEqual("https://www.example.com/news/make-a-wise-decision", node.Url);
        }

        [TestMethod]
        public void DeepNodeUrlPrefixSet()
        {
            var parser = new UmbracoXmlParser(_tempFile, new Dictionary<int, string> { { 1069, "https://www.example.com" } });
            var node = parser.GetNode(2448);
            Assert.AreEqual("https://www.example.com/news/oct-2014/people-with-bad-credit-score", node.Url);
        }

        [TestMethod]
        public void DeepNodeUrlPrefixSetWithTrailingSlash()
        {
            var parser = new UmbracoXmlParser(_tempFile, new Dictionary<int, string> { { 1069, "https://www.example.com/" } });
            var node = parser.GetNode(2448);
            Assert.AreEqual("https://www.example.com/news/oct-2014/people-with-bad-credit-score", node.Url);
        }

        [TestMethod]
        public void GetPropertyAsString()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2552);
            Assert.AreEqual("This is a long string with special < > characters.", node.GetPropertyAsString("stringField"));
        }

        [TestMethod]
        public void GetPropertyAsBool_False()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2552);
            Assert.IsFalse(node.GetPropertyAsBool("boolFieldFalse"));
        }

        [TestMethod]
        public void GetPropertyAsBool_True()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2552);
            Assert.IsTrue(node.GetPropertyAsBool("boolFieldTrue"));
        }

        [TestMethod]
        public void GetPropertyAsInt()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2552);
            Assert.AreEqual(2048, node.GetPropertyAsInt("intField"));
        }

        [TestMethod]
        public void GetPropertyAsDate()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2552);
            Assert.AreEqual(new DateTime(2015, 5, 22, 12, 10, 22), node.GetPropertyAsDate("dateField"));
        }

        [TestMethod]
        public void GetPropertyAsXmlString()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2552);
            Assert.AreEqual(@"<nodes>
  <node>1</node>
  <node>2</node>
  <node>3</node>
</nodes>", node.GetPropertyAsXmlString("xmlField"));
        }

        [TestMethod]
        public void EnumerateNodes()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var enumerator = parser.GetNodes().GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual(1069, enumerator.Current.Id);
            Assert.AreEqual(null, enumerator.Current.ParentId);
            Assert.AreEqual("SiteRoot", enumerator.Current.Doctype);
            Assert.AreEqual(1, enumerator.Current.Level);
            Assert.AreEqual("Example Site", enumerator.Current.Name);
            Assert.AreEqual("example-site", enumerator.Current.Url);
            Assert.AreEqual(new DateTime(2014, 12, 12, 13, 23, 29), enumerator.Current.CreateDate);
            Assert.AreEqual(new DateTime(2016, 9, 1, 16, 45, 19), enumerator.Current.UpdateDate);
            Assert.AreEqual("admin", enumerator.Current.CreatorName);
            Assert.AreEqual("james", enumerator.Current.WriterName);
            Assert.AreEqual(1067, enumerator.Current.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1072, enumerator.Current.Id);
            Assert.AreEqual(1069, enumerator.Current.ParentId);
            Assert.AreEqual("Homepage", enumerator.Current.Doctype);
            Assert.AreEqual(2, enumerator.Current.Level);
            Assert.AreEqual("Homepage", enumerator.Current.Name);
            Assert.AreEqual("example-site/homepage", enumerator.Current.Url);
            Assert.AreEqual(new DateTime(2014, 12, 15, 10, 1, 20), enumerator.Current.CreateDate);
            Assert.AreEqual(new DateTime(2016, 8, 30, 10, 16, 30), enumerator.Current.UpdateDate);
            Assert.AreEqual("admin", enumerator.Current.CreatorName);
            Assert.AreEqual("fred", enumerator.Current.WriterName);
            Assert.AreEqual(1067, enumerator.Current.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069, 1072 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site", "Homepage" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(2552, enumerator.Current.Id);
            Assert.AreEqual(1069, enumerator.Current.ParentId);
            Assert.AreEqual("Content", enumerator.Current.Doctype);
            Assert.AreEqual(2, enumerator.Current.Level);
            Assert.AreEqual("Content", enumerator.Current.Name);
            Assert.AreEqual("example-site/content", enumerator.Current.Url);
            Assert.AreEqual(new DateTime(2015, 5, 22, 12, 10, 22), enumerator.Current.CreateDate);
            Assert.AreEqual(new DateTime(2016, 5, 18, 5, 44, 15), enumerator.Current.UpdateDate);
            Assert.AreEqual("sally", enumerator.Current.CreatorName);
            Assert.AreEqual("admin", enumerator.Current.WriterName);
            Assert.AreEqual(1067, enumerator.Current.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069, 2552 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site", "Content" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1239, enumerator.Current.Id);
            Assert.AreEqual(1069, enumerator.Current.ParentId);
            Assert.AreEqual("CategoryPage", enumerator.Current.Doctype);
            Assert.AreEqual(2, enumerator.Current.Level);
            Assert.AreEqual("News", enumerator.Current.Name);
            Assert.AreEqual("example-site/news", enumerator.Current.Url);
            Assert.AreEqual(new DateTime(2015, 3, 25, 13, 42, 15), enumerator.Current.CreateDate);
            Assert.AreEqual(new DateTime(2016, 3, 1, 5, 39, 24), enumerator.Current.UpdateDate);
            Assert.AreEqual("admin", enumerator.Current.CreatorName);
            Assert.AreEqual("admin", enumerator.Current.WriterName);
            Assert.AreEqual(1067, enumerator.Current.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069, 1239 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site", "News" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(2447, enumerator.Current.Id);
            Assert.AreEqual(1239, enumerator.Current.ParentId);
            Assert.AreEqual("DateFolder", enumerator.Current.Doctype);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("Oct 2014", enumerator.Current.Name);
            Assert.AreEqual("example-site/news/oct-2014", enumerator.Current.Url);
            Assert.AreEqual(new DateTime(2015, 5, 13, 12, 10, 32), enumerator.Current.CreateDate);
            Assert.AreEqual(new DateTime(2015, 5, 13, 12, 10, 32), enumerator.Current.UpdateDate);
            Assert.AreEqual("admin", enumerator.Current.CreatorName);
            Assert.AreEqual("admin", enumerator.Current.WriterName);
            Assert.AreEqual(0, enumerator.Current.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069, 1239, 2447 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site", "News", "Oct 2014" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(2448, enumerator.Current.Id);
            Assert.AreEqual(2447, enumerator.Current.ParentId);
            Assert.AreEqual("Article", enumerator.Current.Doctype);
            Assert.AreEqual(4, enumerator.Current.Level);
            Assert.AreEqual("People with No or Bad Credit Score", enumerator.Current.Name);
            Assert.AreEqual("example-site/news/oct-2014/people-with-bad-credit-score", enumerator.Current.Url);
            Assert.AreEqual(new DateTime(2015, 5, 13, 12, 10, 33), enumerator.Current.CreateDate);
            Assert.AreEqual(new DateTime(2015, 10, 22, 7, 42, 8), enumerator.Current.UpdateDate);
            Assert.AreEqual("angela", enumerator.Current.CreatorName);
            Assert.AreEqual("admin", enumerator.Current.WriterName);
            Assert.AreEqual(1067, enumerator.Current.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069, 1239, 2447, 2448 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site", "News", "Oct 2014", "People with No or Bad Credit Score" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(2499, enumerator.Current.Id);
            Assert.AreEqual(2447, enumerator.Current.ParentId);
            Assert.AreEqual("Article", enumerator.Current.Doctype);
            Assert.AreEqual(4, enumerator.Current.Level);
            Assert.AreEqual("Make a Wise Decision by Comparing Price Online", enumerator.Current.Name);
            Assert.AreEqual("news/make-a-wise-decision", enumerator.Current.Url);
            Assert.AreEqual(new DateTime(2015, 5, 15, 10, 30, 43), enumerator.Current.CreateDate);
            Assert.AreEqual(new DateTime(2015, 6, 18, 16, 38, 31), enumerator.Current.UpdateDate);
            Assert.AreEqual("anish", enumerator.Current.CreatorName);
            Assert.AreEqual("anish", enumerator.Current.WriterName);
            Assert.AreEqual(1067, enumerator.Current.TemplateId);
            CollectionAssert.AreEqual(new[] { 1069, 1239, 2447, 2499 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Example Site", "News", "Oct 2014", "Make a Wise Decision by Comparing Price Online" }, enumerator.Current.PathNames);
        }

        [TestMethod]
        public void GetProperties()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2552);
            var dictionary = node.GetProperties();
            Assert.AreEqual(9, dictionary.Keys.Count);
            Assert.AreEqual("2015-05-22T12:10:22", dictionary["dateField"]);
            Assert.AreEqual("0", dictionary["boolFieldFalse"]);
            Assert.AreEqual("1", dictionary["boolFieldTrue"]);
            Assert.AreEqual("2048", dictionary["intField"]);
            Assert.AreEqual("<nodes><node>1</node><node>2</node><node>3</node></nodes>", dictionary["xmlField"]);
            Assert.AreEqual("This is a long string with special < > characters.", dictionary["stringField"]);
        }

        [TestMethod]
        public void TestEscapingAndCdata()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2552);
            var dictionary = node.GetProperties();

            // <escapedString>Jack &amp; Jill</escapedString>
            // <cdataString><![CDATA[Jack & Jill]]></cdataString>
            // <cdataEscapedString><![CDATA[Jack &amp; Jill]]></cdataEscapedString>
            Assert.AreEqual("Jack & Jill", dictionary["escapedString"]);
            Assert.AreEqual("Jack & Jill", node.GetPropertyAsString("escapedString"));
            Assert.AreEqual("Jack & Jill", dictionary["cdataString"]);
            Assert.AreEqual("Jack & Jill", node.GetPropertyAsString("cdataString"));
            Assert.AreEqual("Jack &amp; Jill", dictionary["cdataEscapedString"]);
            Assert.AreEqual("Jack &amp; Jill", node.GetPropertyAsString("cdataEscapedString"));
        }

        [TestMethod]
        public void GetChildren()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(2447);
            var children = node.Children;

            Assert.AreEqual(2, children.Count());
            Assert.AreEqual(2448, children.ElementAt(0).Id);
            Assert.AreEqual(2499, children.ElementAt(1).Id);
        }

        private string GetUmbracoConfigFromResource(string resourceName = "umbraco.config")
        {
            FileStream fs;
            var tempFile = Path.GetTempFileName();
            using (fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096))
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RecursiveMethod.UmbracoXmlParser.UnitTests.Resources." + resourceName);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fs);
                fs.Close();
            }
            return tempFile;
        }
    }
}
