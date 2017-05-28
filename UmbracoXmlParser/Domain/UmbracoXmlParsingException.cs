using System;
using System.Runtime.Serialization;

namespace RecursiveMethod.UmbracoXmlParser.Domain
{
    public class UmbracoXmlParsingException : Exception
    {
        public UmbracoXmlParsingException()
        {
        }

        public UmbracoXmlParsingException(string message) : base(message)
        {
        }

        public UmbracoXmlParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UmbracoXmlParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
