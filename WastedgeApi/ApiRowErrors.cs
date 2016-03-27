using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class ApiRowErrors
    {
        public int Row { get; }
        public ApiErrorCollection Errors { get; }

        public ApiRowErrors(int row)
            : this(row, new ApiErrorCollection())
        {
        }

        private ApiRowErrors(int row, ApiErrorCollection errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            Row = row;
            Errors = errors;
        }

        internal static ApiRowErrors FromJson(JObject row)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            return new ApiRowErrors(
                (int)row["row"],
                ApiErrorCollection.FromJson((JArray)row["errors"])
            );
        }
    }
}
