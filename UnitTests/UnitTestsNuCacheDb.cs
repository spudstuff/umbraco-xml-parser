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
    public class UnitTestsNuCacheDb
    {
        private string _tempFile;

        [TestInitialize]
        public void Initialize()
        {
            _tempFile = GetNuCacheDbFromResource();
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
        public void RootNodePropertiesSet()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1095);
            Assert.AreEqual(1095, node.Id);
            Assert.AreEqual("ca4249ed2b234337b52263cabe5587d1", node.Uid);
            Assert.IsNull(node.ParentId);
            Assert.AreEqual("1089", node.Doctype);
            Assert.AreEqual(1, node.Level);
            Assert.AreEqual("Home", node.Name);
            Assert.AreEqual("home", node.Url);
            Assert.AreEqual(new DateTime(2019, 6, 25, 8, 4, 16), node.CreateDate);
            Assert.AreEqual(new DateTime(2019, 6, 25, 14, 5, 34), node.UpdateDate);
            Assert.AreEqual("-1", node.CreatorName);
            Assert.AreEqual("-1", node.WriterName);
            Assert.AreEqual(1076, node.TemplateId);
            CollectionAssert.AreEqual(new[] { 1095 }, node.PathIds);
            CollectionAssert.AreEqual(new[] { "Home" }, node.PathNames);
            
            // Parent
            Assert.IsNull(node.Parent);
        }

        [TestMethod]
        public void RootNodePropertiesSetWithDoctypeAndUserMappings()
        {
            var parser = new UmbracoXmlParser(_tempFile, new UmbracoParsingOptions
            {
                UrlPrefixMapping = new Dictionary<int, string> { { 1095, "https://www.example.com/" } },
                DoctypeMapping = new Dictionary<int, string> { { 1095, "HomeDoctype" } },
                UserMapping = new Dictionary<int, string> { { -1, "admin" } }
            });
            var node = parser.GetNode(1095);
            Assert.AreEqual(1095, node.Id);
            Assert.AreEqual("ca4249ed2b234337b52263cabe5587d1", node.Uid);
            Assert.IsNull(node.ParentId);
            Assert.AreEqual("HomeDoctype", node.Doctype);
            Assert.AreEqual(1, node.Level);
            Assert.AreEqual("Home", node.Name);
            Assert.AreEqual("https://www.example.com", node.Url);
            Assert.AreEqual(new DateTime(2019, 6, 25, 8, 4, 16), node.CreateDate);
            Assert.AreEqual(new DateTime(2019, 6, 25, 14, 5, 34), node.UpdateDate);
            Assert.AreEqual("admin", node.CreatorName);
            Assert.AreEqual("admin", node.WriterName);
            Assert.AreEqual(1076, node.TemplateId);
            CollectionAssert.AreEqual(new[] { 1095 }, node.PathIds);
            CollectionAssert.AreEqual(new[] { "Home" }, node.PathNames);
            
            // Parent
            Assert.IsNull(node.Parent);
        }

        [TestMethod]
        public void GetNodeByGuid()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode("ec4aafcc0c254f25a8fe705bfae1d324");
            Assert.AreEqual(1096, node.Id);
            Assert.AreEqual("ec4aafcc0c254f25a8fe705bfae1d324", node.Uid);
            Assert.AreEqual(1095, node.ParentId);
            Assert.AreEqual("1085", node.Doctype);
            Assert.AreEqual(2, node.Level);
            Assert.AreEqual("Products", node.Name);
            Assert.AreEqual("home/products", node.Url);
            Assert.AreEqual(new DateTime(2019, 6, 25, 8, 4, 16), node.CreateDate);
            Assert.AreEqual(new DateTime(2019, 6, 25, 8, 4, 17), node.UpdateDate);
            Assert.AreEqual("-1", node.CreatorName);
            Assert.AreEqual("-1", node.WriterName);
            Assert.AreEqual(1081, node.TemplateId);
            CollectionAssert.AreEqual(new[] { 1095, 1096 }, node.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products" }, node.PathNames);
            
            // Confirm in actual GUID format
            node = parser.GetNode("EC4AAFCC-0C25-4F25-A8FE-705BFAE1D324");
            Assert.AreEqual(1096, node.Id);
        }

        [TestMethod]
        public void DeepNodePropertiesSet()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1097);
            Assert.AreEqual(1097, node.Id);
            Assert.AreEqual("df1eb830411b4d41a3433917b76d533c", node.Uid);
            Assert.AreEqual(1096, node.ParentId);
            Assert.AreEqual("1086", node.Doctype);
            Assert.AreEqual(3, node.Level);
            Assert.AreEqual("Tattoo", node.Name);
            Assert.AreEqual("home/products/tattoo", node.Url);
            Assert.AreEqual(new DateTime(2019, 6, 25, 8, 4, 16), node.CreateDate);
            Assert.AreEqual(new DateTime(2019, 6, 25, 8, 4, 17), node.UpdateDate);
            Assert.AreEqual("-1", node.CreatorName);
            Assert.AreEqual("-1", node.WriterName);
            Assert.AreEqual(1080, node.TemplateId);
            CollectionAssert.AreEqual(new [] { 1095, 1096, 1097 }, node.PathIds);
            CollectionAssert.AreEqual(new [] { "Home", "Products", "Tattoo" }, node.PathNames);

            // Parent
            Assert.AreEqual(1096, node.Parent.Id);
            Assert.AreEqual("ec4aafcc0c254f25a8fe705bfae1d324", node.Parent.Uid);
            Assert.AreEqual(1095, node.Parent.ParentId);
            Assert.AreEqual("1085", node.Parent.Doctype);
            Assert.AreEqual(2, node.Parent.Level);
            Assert.AreEqual("Products", node.Parent.Name);
            Assert.AreEqual("home/products", node.Parent.Url);
            Assert.AreEqual(new DateTime(2019, 6, 25, 8, 4, 16), node.Parent.CreateDate);
            Assert.AreEqual(new DateTime(2019, 6, 25, 8, 4, 17), node.Parent.UpdateDate);
            Assert.AreEqual("-1", node.Parent.CreatorName);
            Assert.AreEqual("-1", node.Parent.WriterName);
            Assert.AreEqual(1081, node.Parent.TemplateId);
            CollectionAssert.AreEqual(new[] { 1095, 1096 }, node.Parent.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products" }, node.Parent.PathNames);
        }

        [TestMethod]
        public void RootNodeUrlPrefixSet()
        {
            var parser = new UmbracoXmlParser(_tempFile, new Dictionary<int, string> { { 1095, "https://www.example.com"} });
            var node = parser.GetNode(1095);
            Assert.AreEqual("https://www.example.com", node.Url);
        }

        [TestMethod]
        public void DeepNodeUrlPrefixSet()
        {
            var parser = new UmbracoXmlParser(_tempFile, new Dictionary<int, string> { { 1095, "https://www.example.com" } });
            var node = parser.GetNode(1097);
            Assert.AreEqual("https://www.example.com/products/tattoo", node.Url);
        }

        [TestMethod]
        public void DeepNodeUrlPrefixSetWithTrailingSlash()
        {
            var parser = new UmbracoXmlParser(_tempFile, new Dictionary<int, string> { { 1095, "https://www.example.com/" } });
            var node = parser.GetNode(1097);
            Assert.AreEqual("https://www.example.com/products/tattoo", node.Url);
        }

        [TestMethod]
        public void GetPropertyAsString()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1095);
            Assert.AreEqual("Check our products", node.GetPropertyAsString("heroCTACaption"));
        }

        [TestMethod]
        public void GetPropertyAsBool_False()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1105);
            Assert.IsFalse(node.GetPropertyAsBool("umbracoNavihide"));
        }

        [TestMethod]
        public void GetPropertyAsBool_True()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1106);
            Assert.IsTrue(node.GetPropertyAsBool("umbracoNavihide"));
        }

        [TestMethod]
        public void GetPropertyAsInt()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1114);
            Assert.AreEqual(2, node.GetPropertyAsInt("howManyPostsShouldBeShown"));
        }

        [TestMethod]
        public void GetPropertyAsDate()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1117);
            Assert.AreEqual(new DateTime(2019, 10, 19, 13, 15, 0), node.GetPropertyAsDate("publishedDate"));
        }

        [TestMethod]
        public void EnumerateNodes()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var enumerator = parser.GetNodes().GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual(1095, enumerator.Current.Id);
            Assert.AreEqual(1, enumerator.Current.Level);
            Assert.AreEqual("home", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1096, enumerator.Current.Id);
            Assert.AreEqual(2, enumerator.Current.Level);
            Assert.AreEqual("home/products", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1097, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/products/tattoo", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096, 1097 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products", "Tattoo" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1098, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/products/unicorn", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096, 1098 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products", "Unicorn" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1099, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/products/ping-pong-ball", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096, 1099 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products", "Ping Pong Ball" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1100, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/products/bowling-ball", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096, 1100 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products", "Bowling Ball" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1101, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/products/jumpsuit", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096, 1101 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products", "Jumpsuit" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1102, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/products/banjo", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096, 1102 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products", "Banjo" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1103, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/products/knitted-west", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096, 1103 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products", "Knitted West" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1104, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/products/biker-jacket", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1096, 1104 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Products", "Biker Jacket" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1105, enumerator.Current.Id);
            Assert.AreEqual(2, enumerator.Current.Level);
            Assert.AreEqual("home/people", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1105 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "People" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1106, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/people/jan-skovgaard", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1105, 1106 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "People", "Jan Skovgaard" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1107, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/people/matt-brailsford", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1105, 1107 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "People", "Matt Brailsford" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1108, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/people/lee-kelleher", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1105, 1108 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "People", "Lee Kelleher" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1109, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/people/jeavon-leopold", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1105, 1109 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "People", "Jeavon Leopold" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1110, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/people/jeroen-breuer", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1105, 1110 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "People", "Jeroen Breuer" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1111, enumerator.Current.Id);
            Assert.AreEqual(2, enumerator.Current.Level);
            Assert.AreEqual("home/about-us", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1111 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "About Us" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1112, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/about-us/about-this-starter-kit", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1111, 1112 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "About Us", "About this Starter Kit" }, enumerator.Current.PathNames);
            
            enumerator.MoveNext();
            Assert.AreEqual(1113, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/about-us/todo-list-for-the-starter-kit", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1111, 1113 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "About Us", "Todo list for the Starter Kit" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1114, enumerator.Current.Id);
            Assert.AreEqual(2, enumerator.Current.Level);
            Assert.AreEqual("home/blog", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1114 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Blog" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1115, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/blog/my-blog-post", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1114, 1115 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Blog", "My Blog Post" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1116, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/blog/another-one", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1114, 1116 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Blog", "Another one" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1117, enumerator.Current.Id);
            Assert.AreEqual(3, enumerator.Current.Level);
            Assert.AreEqual("home/blog/this-will-be-great", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1114, 1117 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Blog", "This will be great" }, enumerator.Current.PathNames);

            enumerator.MoveNext();
            Assert.AreEqual(1118, enumerator.Current.Id);
            Assert.AreEqual(2, enumerator.Current.Level);
            Assert.AreEqual("home/contact", enumerator.Current.Url);
            CollectionAssert.AreEqual(new[] { 1095, 1118 }, enumerator.Current.PathIds);
            CollectionAssert.AreEqual(new[] { "Home", "Contact" }, enumerator.Current.PathNames);
        }

        [TestMethod]
        public void GetProperties()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1095);
            var dictionary = node.GetProperties();
            Assert.AreEqual(15, dictionary.Keys.Count);
            Assert.AreEqual("Umbraco Demo", dictionary["heroHeader"]);
            Assert.AreEqual("Moonfish, steelhead, lamprey southern flounder tadpole fish sculpin bigeye, blue-redstripe danio collared dogfish. Smalleye squaretail goldfish arowana butterflyfish pipefish wolf-herring jewel tetra, shiner; gibberfish red velvetfish. Thornyhead yellowfin pike threadsail ayu cutlassfish.", dictionary["heroDescription"]);
            Assert.AreEqual("Check our products", dictionary["heroCTACaption"]);
            Assert.AreEqual("umb://document/ec4aafcc0c254f25a8fe705bfae1d324", dictionary["HeroCtalink"]);
            Assert.AreEqual("{\r\n  \"make\": \"Holden\",\r\n  \"model\": \"Commodore\"\r\n}", dictionary["g5amx923cj7"]);
            Assert.IsTrue(dictionary["bodyText"].StartsWith("{") && dictionary["bodyText"].EndsWith("}"));
            Assert.AreEqual("Umbraco Demo", dictionary["footerHeader"]);
            Assert.AreEqual("Curabitur arcu erat, accumsan id imperdiet et, porttitor at sem. Curabitur arcu erat, accumsan id imperdiet et, porttitor at sem. Vivamus suscipit tortor eget felis porttitor volutpat", dictionary["footerDescription"]);
            Assert.AreEqual("Read All on the Blog", dictionary["footerCTACaption"]);
            Assert.AreEqual("umb://document/1d770f10d1ca4a269d68071e2c9f7ac1", dictionary["FooterCtalink"]);
            Assert.AreEqual("Umbraco HQ - Unicorn Square - Haubergsvej 1 - 5000 Odense C - Denmark - +45 70 26 11 62", dictionary["footerAddress"]);
            Assert.AreEqual("umb://media/662af6ca411a4c93a6c722c4845698e7", dictionary["HeroBackgroundImage"]);
            Assert.AreEqual("serif", dictionary["font"]);
            Assert.AreEqual("earth", dictionary["colorTheme"]);
            Assert.AreEqual("Umbraco Sample Site", dictionary["sitename"]);
        }

        [TestMethod]
        public void GetTypedProperties()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1114);
            var dictionary = node.GetTypedProperties();
            Assert.AreEqual(5, dictionary.Keys.Count);
            foreach (var key in dictionary.Keys)
            {
                Console.WriteLine("{0} ({1}) = {2}", key, dictionary[key].GetType().Name, dictionary[key]);
            }
            Assert.AreEqual("Behind The Scenes", dictionary["pageTitle"]);
            Assert.AreEqual("{\n  \"name\": \"1 column layout\",\n  \"sections\": [\n    {\n      \"grid\": 12,\n      \"allowAll\": true,\n      \"rows\": []\n    }\n  ]\n}", dictionary["bodyText"]);
            Assert.AreEqual("[]", dictionary["keywords"]);
            Assert.AreEqual(0, dictionary["umbracoNavihide"]);
            Assert.AreEqual("2", dictionary["howManyPostsShouldBeShown"]);

            Assert.IsTrue(dictionary["pageTitle"] is String);
            Assert.IsTrue(dictionary["bodyText"] is String);
            Assert.IsTrue(dictionary["keywords"] is String);
            Assert.IsTrue(dictionary["umbracoNavihide"] is Int32);
            Assert.IsTrue(dictionary["howManyPostsShouldBeShown"] is String);
        }
        
        [TestMethod]
        public void GetChildren()
        {
            var parser = new UmbracoXmlParser(_tempFile);
            var node = parser.GetNode(1105);
            var children = node.Children;

            Assert.AreEqual(5, children.Count());
            Assert.AreEqual(1106, children.ElementAt(0).Id);
            Assert.AreEqual(1107, children.ElementAt(1).Id);
            Assert.AreEqual(1108, children.ElementAt(2).Id);
            Assert.AreEqual(1109, children.ElementAt(3).Id);
            Assert.AreEqual(1110, children.ElementAt(4).Id);
        }

        private string GetNuCacheDbFromResource(string resourceName = "NuCache.Content.db")
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
