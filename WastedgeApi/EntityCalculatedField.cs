using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public class EntityCalculatedField : EntityTypedField
    {
        public override EntityMemberType Type => EntityMemberType.Calculated;

        public EntityCalculatedField(string name, string comments, EntityDataType dataType, int? decimals)
            : base(name, comments, dataType, decimals)
        {
        }
    }
}
