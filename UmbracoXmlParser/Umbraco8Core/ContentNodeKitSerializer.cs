using System;
using System.IO;
using CSharpTest.Net.Serialization;

namespace RecursiveMethod.UmbracoXmlParser.Umbraco8Core
{
    internal class ContentNodeKitSerializer : ISerializer<ContentNodeKit>
    {
        static readonly ContentDataSerializer DataSerializer = new ContentDataSerializer();

        public ContentNodeKit ReadFrom(Stream stream)
        {
            var kit = new ContentNodeKit
            {
                Node = new ContentNode(PrimitiveSerializer.Int32.ReadFrom(stream), // id
                    PrimitiveSerializer.Guid.ReadFrom(stream), // uid
                    PrimitiveSerializer.Int32.ReadFrom(stream), // level
                    PrimitiveSerializer.String.ReadFrom(stream), // path
                    PrimitiveSerializer.Int32.ReadFrom(stream), // sort order
                    PrimitiveSerializer.Int32.ReadFrom(stream), // parent id
                    PrimitiveSerializer.DateTime.ReadFrom(stream), // date created
                    PrimitiveSerializer.Int32.ReadFrom(stream) // creator id
                ),
                ContentTypeId = PrimitiveSerializer.Int32.ReadFrom(stream)
            };
            var hasDraft = PrimitiveSerializer.Boolean.ReadFrom(stream);
            if (hasDraft)
            {
                kit.DraftData = DataSerializer.ReadFrom(stream);
            }
            var hasPublished = PrimitiveSerializer.Boolean.ReadFrom(stream);
            if (hasPublished)
            {
                kit.PublishedData = DataSerializer.ReadFrom(stream);
            }
            return kit;
        }

        public void WriteTo(ContentNodeKit value, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
