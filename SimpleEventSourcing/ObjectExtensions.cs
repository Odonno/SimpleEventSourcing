using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing
{
    public static class ObjectExtensions
    {
        public static T ConvertTo<T>(this object o)
        {
            if (o is JObject jsonObject)
            {
                return jsonObject.ToObject<T>();
            }

            Type objectType = o.GetType();
            Type target = typeof(T);

            var x = Activator.CreateInstance(target, false);

            var d = from source in target.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;

            List<MemberInfo> members = d
                .Where(memberInfo => d.Select(c => c.Name).ToList().Contains(memberInfo.Name))
                .ToList();

            foreach (var memberInfo in members)
            {
                var propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                var value = o.GetType().GetProperty(memberInfo.Name).GetValue(o, null);

                propertyInfo.SetValue(x, value, null);
            }

            return (T)x;
        }
    }
}
