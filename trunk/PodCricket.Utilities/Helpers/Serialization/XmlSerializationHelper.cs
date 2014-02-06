using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PodCricket.Utilities.Helpers.Serialization
{
    public class XmlSerializationHelper : SerializationHelper
    {
        protected override T DoDeserialize<T>(Stream fs)
        {
            var serializer = new XmlSerializer(typeof(T));
            var cache = serializer.Deserialize(fs);
            return (T)cache;
        }

        protected override bool DoSerialize(Stream stream, object graph)
        {
            if (graph == null)
                return false;

            var serializer = new XmlSerializer(graph.GetType());
            if (stream != null)
            {
                serializer.Serialize(stream, graph);
                return true;
            }

            return false;
        }
    }
}
