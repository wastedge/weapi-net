using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class ApiError
    {
        public string Field { get; }
        public string Error { get; }

        public ApiError(string error)
            : this(null, error)
        {
        }

        public ApiError(string field, string error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            Field = field;
            Error = error;
        }

        internal static ApiError FromJson(JToken error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            return new ApiError(
                (string)error["field"],
                (string)error["error"]
            );
        }
    }
}
