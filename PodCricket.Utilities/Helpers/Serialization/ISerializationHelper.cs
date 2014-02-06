using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.Utilities.Helpers.Serialization
{
    public interface ISerializationHelper
    {
        Task<T> DeserializeAsync<T>(Stream stream) where T : class;
        Task<bool> SerializeAsync(Stream stream, object graph);

        T Deserialize<T>(Stream stream) where T : class;
        bool Serialize(Stream stream, object graph);
    }
}
