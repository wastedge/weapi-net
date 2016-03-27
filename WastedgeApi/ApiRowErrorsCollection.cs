using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class ApiRowErrorsCollection : Collection<ApiRowErrors>
    {
        internal static ApiRowErrorsCollection FromJson(JArray errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            var result = new ApiRowErrorsCollection();

            foreach (var row in errors)
            {
                if (row.Type == JTokenType.Object)
                    result.Add(ApiRowErrors.FromJson((JObject)row));
            }

            return result;
        }
    }
}
