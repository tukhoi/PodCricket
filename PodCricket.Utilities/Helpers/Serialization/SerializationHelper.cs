using Microsoft.Phone.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PodCricket.Utilities.Helpers.Serialization
{
    public abstract class SerializationHelper : ISerializationHelper
    {
        #region Virtual public methods

        public virtual async Task<T> DeserializeAsync<T>(Stream stream)
           where T : class
        {
            T cache = default(T);
            try
            {
                if (stream != null)
                    cache = await Task.Run(() => DoDeserialize<T>(stream));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (stream != null)
                    stream.Close();

            }
            return cache;
        }

        public virtual async Task<bool> SerializeAsync(Stream stream, object graph)
        {
            bool result = false;
            try
            {
                if (stream != null)
                    result = await Task.Run(() => DoSerialize(stream, graph));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return result;
        }

        public virtual T Deserialize<T>(Stream stream)
           where T : class
        {
            T cache = default(T);
            try
            {
                if (stream != null)
                    cache = DoDeserialize<T>(stream);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (stream != null)
                    stream.Close();

            }
            return cache;
        }

        public virtual bool Serialize(Stream stream, object graph)
        {
            bool result = false;
            try
            {
                if (stream != null)
                    result = DoSerialize(stream, graph);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return result;
        }

        #endregion

        #region abstract medthods

        protected abstract T DoDeserialize<T>(Stream fs);
        protected abstract bool DoSerialize(Stream fs, object graph);


        //protected virtual T DoDeserialize<T>(Stream fs)
        //{
        //    throw new NotImplementedException();
        //}

        //protected virtual bool DoSerialize(Stream stream, object graph)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}
