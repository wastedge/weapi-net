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

        internal Schema(IList<string> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            Entities = new ReadOnlyCollection<string>(entities);
        }
    }
}
