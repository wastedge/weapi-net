using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public class EntityForeignChild : EntityMember
    {
        public string LinkTable { get; }
        public string LinkField { get; }

        public override EntityMemberType Type => EntityMemberType.ForeignChild;

        public EntityForeignChild(string name, string comments, string linkTable, string linkField)
            : base(name, comments)
        {
            LinkTable = linkTable;
            LinkField = linkField;
        }
    }
}
