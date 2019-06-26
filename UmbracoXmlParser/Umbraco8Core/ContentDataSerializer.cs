using System;
using System.IO;
using CSharpTest.Net.Serialization;

namespace RecursiveMethod.UmbracoXmlParser.Umbraco8Core
{
    internal class ContentDataSerializer : ISerializer<ContentData>
    {
        private static readonly DictionaryOfPropertyDataSerializer PropertiesSerializer = new DictionaryOfPropertyDataSerializer();
        private static readonly DictionaryOfCultureVariationSerializer CultureVariationsSerializer = new DictionaryOfCultureVariationSerializer();

        public ContentData ReadFrom(Stream stream)
        {
            return new ContentData
            {
                Published = PrimitiveSerializer.Boolean.ReadFrom(stream),
                Name = PrimitiveSerializer.String.ReadFrom(stream),
                UrlSegment = PrimitiveSerializer.String.ReadFrom(stream),
                VersionId = PrimitiveSerializer.Int32.ReadFrom(stream),
                VersionDate = PrimitiveSerializer.DateTime.ReadFrom(stream),
                WriterId = PrimitiveSerializer.Int32.ReadFrom(stream),
                TemplateId = PrimitiveSerializer.Int32.ReadFrom(stream),
                Properties = PropertiesSerializer.ReadFrom(stream),
                CultureInfos = CultureVariationsSerializer.ReadFrom(stream)
            };
        }

        public void WriteTo(ContentData value, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
