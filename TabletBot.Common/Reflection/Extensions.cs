using System;
using System.Linq;
using System.Reflection;

namespace TabletBot.Common.Reflection
{
    public static class Extensions
    {
        public static T Construct<T>(this TypeInfo type)
        {
            var ctor = type.GetConstructor(new Type[0]);
            return (T)ctor.Invoke(new object[0]);
        }
    }
}