using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.Utilities.Helpers.Serialization
{
    public class BinaryFormatterHelper : SerializationHelper
    {
        protected override T DoDeserialize<T>(Stream fs)
        {
            //var formatter = new BinaryFormatter();
            //var cache = formatter.Deserialize(fs);
            //return (T)cache;

            //throw new NotImplementedException();

            return default(T);
        }

        protected override bool DoSerialize(Stream stream, object graph)
        {
            //var formatter = new BinaryFormatter();
            //if (stream != null)
            //    formatter.Serialize(stream, graph);

            //throw new NotImplementedException();

            return false;
        }
    }
}
