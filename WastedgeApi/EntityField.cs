using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public class EntityField : EntityPhysicalField
    {
        public override EntityMemberType Type => EntityMemberType.Field;

        public EntityField(string name, string comments, EntityDataType dataType, bool mandatory)
            : base(name, comments, dataType, mandatory)
        {
        }
    }
}
