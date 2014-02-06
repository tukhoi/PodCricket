
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.Utilities.ApplicationServices
{
    public class BaseResult<T, TError>
    {
        public T Target { get; set; }
        public TError Error { get; set; }

        public BaseResult(T target, TError error)
        {
            Target = target;
            Error = error;
        }

        public BaseResult(TError error) : this(default(T), error)
        {

        }
    }
}
