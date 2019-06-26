using System;
using System.Collections.Generic;
using System.IO;
using CSharpTest.Net.Serialization;

namespace RecursiveMethod.UmbracoXmlParser.Umbraco8Core
{
    internal class DictionaryOfCultureVariationSerializer : SerializerBase, ISerializer<IReadOnlyDictionary<string, CultureVariation>>
    {
        private static readonly IReadOnlyDictionary<string, CultureVariation> Empty = new Dictionary<string, CultureVariation>();

        public IReadOnlyDictionary<string, CultureVariation> ReadFrom(Stream stream)
        {
            // read variations count
            var pcount = PrimitiveSerializer.Int32.ReadFrom(stream);
            if (pcount == 0)
            {
                return Empty;
            }

            // read each variation
            var dict = new Dictionary<string, CultureVariation>();
            for (var i = 0; i < pcount; i++)
            {
                var languageId = PrimitiveSerializer.String.ReadFrom(stream);
                var cultureVariation = new CultureVariation { Name = ReadStringObject(stream), UrlSegment = ReadStringObject(stream), Date = ReadDateTime(stream) };
                dict[languageId] = cultureVariation;
            }
            return dict;
        }

        public void WriteTo(IReadOnlyDictionary<string, CultureVariation> value, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
