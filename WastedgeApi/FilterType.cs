using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WastedgeApi
{
    public enum FilterType
    {
        IsNull,
        NotIsNull,
        IsTrue,
        NotIsTrue,
        IsFalse,
        NotIsFalse,
        In,
        NotIn,
        Like,
        NotLike,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterEqual,
        LessThan,
        LessEqual
    }
}
