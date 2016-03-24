using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public class Filter
    {
        public EntityPhysicalField Field { get; }
        public FilterType Type { get; }
        public object Value { get; }

        public Filter(EntityPhysicalField field, FilterType type, object value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            Field = field;
            Type = type;
            Value = value;
        }
    }
}
