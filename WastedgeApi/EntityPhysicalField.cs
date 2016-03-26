using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public abstract class EntityPhysicalField : EntityTypedField
    {
        public bool Mandatory { get; }

        protected EntityPhysicalField(string name, string comments, EntityDataType dataType, int? decimals, bool mandatory)
            : base(name, comments, dataType, decimals)
        {
            Mandatory = mandatory;
        }
    }
}
