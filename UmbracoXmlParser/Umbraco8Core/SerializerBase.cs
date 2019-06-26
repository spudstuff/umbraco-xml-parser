using System;
using System.IO;
using CSharpTest.Net.Serialization;

namespace RecursiveMethod.UmbracoXmlParser.Umbraco8Core
{
    internal abstract class SerializerBase
    {
        protected string ReadString(Stream stream)
        {
            return PrimitiveSerializer.String.ReadFrom(stream);
        }

        protected int ReadInt(Stream stream)
        {
            return PrimitiveSerializer.Int32.ReadFrom(stream);
        }

        protected long ReadLong(Stream stream)
        {
            return PrimitiveSerializer.Int64.ReadFrom(stream);
        }

        protected float ReadFloat(Stream stream)
        {
            return PrimitiveSerializer.Float.ReadFrom(stream);
        }

        protected double ReadDouble(Stream stream)
        {
            return PrimitiveSerializer.Double.ReadFrom(stream);
        }

        protected DateTime ReadDateTime(Stream stream)
        {
            return PrimitiveSerializer.DateTime.ReadFrom(stream);
        }

        private T? ReadObject<T>(Stream stream, char t, Func<Stream, T> read) where T : struct
        {
            var type = PrimitiveSerializer.Char.ReadFrom(stream);

            if (type == 'N')
            {
                return null;
            }

            if (type != t)
            {
                throw new NotSupportedException($"Cannot deserialize type '{type}', expected '{t}'.");
            }

            return read(stream);
        }

        protected string ReadStringObject(Stream stream) // required 'cos string is not a struct
        {
            var type = PrimitiveSerializer.Char.ReadFrom(stream);

            if (type == 'N')
            {
                return null;
            }

            if (type != 'S')
            {
                throw new NotSupportedException($"Cannot deserialize type '{type}', expected 'S'.");
            }

            return PrimitiveSerializer.String.ReadFrom(stream);
        }

        protected int? ReadIntObject(Stream stream)
        {
            return ReadObject(stream, 'I', ReadInt);
        }

        protected long? ReadLongObject(Stream stream)
        {
            return ReadObject(stream, 'L', ReadLong);
        }

        protected float? ReadFloatObject(Stream stream)
        {
            return ReadObject(stream, 'F', ReadFloat);
        }

        protected double? ReadDoubleObject(Stream stream)
        {
            return ReadObject(stream, 'B', ReadDouble);
        }

        protected DateTime? ReadDateTimeObject(Stream stream)
        {
            return ReadObject(stream, 'D', ReadDateTime);
        }

        protected object ReadObject(Stream stream)
        {
            return ReadObject(PrimitiveSerializer.Char.ReadFrom(stream), stream);
        }

        protected object ReadObject(char type, Stream stream)
        {
            switch (type)
            {
                case 'N':
                    return null;

                case 'S':
                    return PrimitiveSerializer.String.ReadFrom(stream);

                case 'I':
                    return PrimitiveSerializer.Int32.ReadFrom(stream);

                case 'L':
                    return PrimitiveSerializer.Int64.ReadFrom(stream);

                case 'F':
                    return PrimitiveSerializer.Float.ReadFrom(stream);

                case 'B':
                    return PrimitiveSerializer.Double.ReadFrom(stream);

                case 'D':
                    return PrimitiveSerializer.DateTime.ReadFrom(stream);

                default:
                    throw new NotSupportedException($"Cannot deserialize unknown type '{type}'.");
            }
        }

        protected void WriteObject(object value, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
