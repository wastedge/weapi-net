using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class Schema
    {
        public IList<string> Entities { get; }

        internal Schema(JObject schema)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            Entities = new ReadOnlyCollection<string>(
                schema["entities"].Select(p => (string)p).ToList()
            );
        }
    }
}
