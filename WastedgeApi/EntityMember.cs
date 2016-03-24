using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public abstract class EntityMember
    {
        public string Name { get; }
        public string Comments { get; }
        public abstract EntityMemberType Type { get; }

        protected EntityMember(string name, string comments)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Comments = comments;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
