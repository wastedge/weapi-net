using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WastedgeApi
{
    public abstract class EntityTypedField : EntityMember
    {
        public EntityDataType DataType { get; }
        public int? Decimals { get; }

        protected EntityTypedField(string name, string comments, EntityDataType dataType, int? decimals)
            : base(name, comments)
        {
            DataType = dataType;
            Decimals = decimals;
        }
    }
}
