using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WastedgeApi
{
    public class QueryOrder
    {
        public string Field { get; }
        public QueryOrderDirection Direction { get; }

        public QueryOrder(string field)
            : this(field, QueryOrderDirection.Ascending)
        {
        }

        public QueryOrder(string field, QueryOrderDirection direction)
        {
            Field = field;
            Direction = direction;
        }
    }
}
