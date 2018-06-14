using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;

namespace System.Linq
{
    public static class LinqExt
    {
        public static T ToObject<T>(this IDictionary<string, object> source) where T : class, new()
        {
            T someObject = new T();
            Type someObjectType = someObject.GetType();

            foreach (KeyValuePair<string, object> item in source)
            {
                someObjectType.GetProperty(item.Key).SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }

        // for IQueryable
        public static List<IDictionary<string, object>> ToListDictionary(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");

            List<IDictionary<string, object>> ls = new List<IDictionary<string, object>>();

            foreach (var elem in source)
                ls.Add(elem.AsDictionary());

            return ls;
        }


        // for IQueryable
        public static IList ToListDynamicClass(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");

            var returnList = (IList)typeof(List<>)
                .MakeGenericType(source.ElementType)
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);

            foreach (var elem in source)
                returnList.Add(elem);

            return returnList;
        }

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        //used by LINQ to SQL
        public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }

        //used by LINQ
        public static IEnumerable<TSource> Page<TSource>(this IEnumerable<TSource> source, int page, int pageSize)
        {
            return source.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
