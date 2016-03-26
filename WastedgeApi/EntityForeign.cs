using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public class EntityForeign : EntityPhysicalField
    {
        public string LinkTable { get; }

        public override EntityMemberType Type => EntityMemberType.Foreign;

        public EntityForeign(string name, string comments, string linkTable, EntityDataType dataType, int? decimals, bool mandatory)
            : base(name, comments, dataType, decimals, mandatory)
        {
            if (linkTable == null)
                throw new ArgumentNullException(nameof(linkTable));

            LinkTable = linkTable;
        }
    }
}
