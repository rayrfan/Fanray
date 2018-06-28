using System;
using System.Collections.Generic;

namespace Fan.Helpers
{
    public interface ITypeFinder
    {
        IEnumerable<Type> Find<T>();
        IEnumerable<Type> Find(Type baseType);
    }
}
