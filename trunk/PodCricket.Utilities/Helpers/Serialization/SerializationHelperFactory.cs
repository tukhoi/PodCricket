using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.Utilities.Helpers.Serialization
{
    public sealed class SerializationHelperFactory
    {
        readonly IDictionary<SerializationType, ISerializationHelper> innerHelpers;

        internal SerializationHelperFactory()
        {
            innerHelpers = new Dictionary<SerializationType, ISerializationHelper>();

            innerHelpers.Add(SerializationType.BinarySerialization, new BinaryFormatterHelper());
            innerHelpers.Add(SerializationType.XmlSerialization, new XmlSerializationHelper());
            innerHelpers.Add(SerializationType.JsonSerialization, new JsonSerializationHelper());
        }

        public ISerializationHelper GetSerializationHelper(SerializationType type)
        {
            return innerHelpers[type];
        }
    }

    public enum SerializationType
    {
        XmlSerialization,
        BinarySerialization,
        JsonSerialization
    }
}
