using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WastedgeApi
{
    public class EntityIdField : EntityPhysicalField
    {
        public EntityIdField()
            : base("$id", null, EntityDataType.String, true)
        {
        }

        public override EntityMemberType Type => EntityMemberType.Id;
    }
}
