using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class ApiErrorCollection : Collection<ApiError>
    {
        public static ApiErrorCollection FromJson(JArray errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            var result = new ApiErrorCollection();

            foreach (var error in errors)
            {
                result.Add(ApiError.FromJson(error));
            }

            return result;
        }
    }
}
