using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public enum EntityMemberType
    {
        Field,
        Foreign,
        ForeignChild,
        Id,
        Calculated
    }
}
