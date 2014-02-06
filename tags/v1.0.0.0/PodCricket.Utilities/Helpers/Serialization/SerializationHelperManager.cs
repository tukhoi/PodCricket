using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.Utilities.Helpers.Serialization
{
    public class SerializationHelperManager
    {
        static SerializationHelperFactory factory;

        public SerializationHelperManager()
        {
            factory = factory ?? new SerializationHelperFactory();
        }

        public ISerializationHelper GetSerializationHelper(SerializationType type)
        {
            return factory.GetSerializationHelper(type);
        }
    }
}
