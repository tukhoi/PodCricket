using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PodCricket.Utilities.Helpers.Serialization
{
    public class JsonSerializationHelper : SerializationHelper
    {
        protected override T DoDeserialize<T>(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);

            try
            {
                var map = reader.ReadToEnd();
                T graph = JsonConvert.DeserializeObject<T>(map);
                return graph;
            }
            catch (Exception ex)
            {
                return default(T);
            }
            finally {
                reader.Close();
            }
        }

        protected override bool DoSerialize(Stream stream, object graph)
        {
            if (graph == null)
                return false;

            StreamWriter writer = new StreamWriter(stream);

            try
            {
                var map = JsonConvert.SerializeObject(graph);
                writer.Write(map);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally {
                writer.Close();
            }
        }
    }
}
