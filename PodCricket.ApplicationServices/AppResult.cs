
using PodCricket.Utilities.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.ApplicationServices
{
    public class AppResult<T> : BaseResult<T, ErrorCode>
    {
        public AppResult(T target, ErrorCode error) : base(target, error)
        {
        }

        public AppResult(T target) : base(target, ErrorCode.None)
        {
        }

        public AppResult(ErrorCode error) : base(error)
        {
        }

        public bool HasError { get {
            return Error != ErrorCode.None;
        } }

        //public string ErrorMessage { get {
        //    return GetErrorMessage(Error);
        //} }

    }


}
