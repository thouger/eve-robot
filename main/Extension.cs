//using BotEngine.Common;
//using BotEngine.Motor;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Sanderling.Interface.MemoryStruct;
//using Bib3.Geometrik;
//using Bib3;
//using BotEngine.Windows;
using static BotEngine;

namespace Sanderling.Motor
{
    static public class Extension
    {
        //这个函数我不会还原
        //static public bool WhereNotDefault<T>(mvar<u1> element)
        //{
        //    var defaultValue = default(mvar<u1>);
        //    return !object.Equals(defaultValue, element);
        //}

        static public Vektor2DInt? ClientToScreen(this IntPtr hWnd, Vektor2DInt locationInClient)
        {
            var structWinApi = locationInClient.AsWindowsPoint();

            if (!WinApi.ClientToScreen(hWnd, ref structWinApi))
                return null;

            return locationInClient;
        }

        static public IDictionary<KeyT, ValueT> ToDictionary<KeyT, ValueT>(
    this IEnumerable<KeyValuePair<KeyT, ValueT>> source)
        {
            if (null == source)
            {
                return null;
            }

            return source.ToDictionary(t => t.Key, t => t.Value);
        }

        static public IDictionary<KeyT, ValueT[]> ToDictionary<KeyT, ValueT>(
            this IEnumerable<IGrouping<KeyT, ValueT>> source)
        {
            if (null == source)
            {
                return null;
            }

            return
                source.Select(group => new KeyValuePair<KeyT, ValueT[]>(group.Key, group.ToArray()))
                .ToDictionary();
        }
        public static IEnumerable<T> ConcatNullable<T>(
        IEnumerable<IEnumerable<T>> Liste)
        {
            if (null == Liste)
            {
                yield break;
            }

            foreach (var item in Liste)
            {
                if (null == item)
                {
                    continue;
                }

                foreach (var item1 in item)
                {
                    yield return item1;
                }
            }
        }

        public static IEnumerable<T> ConcatNullable<T>(
            IEnumerable<T> Liste0,
            IEnumerable<T> Liste1)
        {
            if (null == Liste0)
            {
                return Liste1;
            }

            if (null == Liste1)
            {
                return Liste0;
            }

            return Liste0.Concat(Liste1);
        }

        public static IEnumerable<T> ConcatNullable2<T>(this IEnumerable<IEnumerable<T>> seq)
        {
            foreach (var item in seq)
            {
                if (item != null)
                {
                    foreach (var subItem in item)
                    {
                        yield return subItem;
                    }
                }
            }
        }

        public static TValue TryGetValueOrDefault<TKey, TValue>(
    this IDictionary<TKey, TValue> Dict,
    TKey Key)
        {
            if (null != Dict)
            {
                TValue Value;

                if (Dict.TryGetValue(Key, out Value))
                {
                    return Value;
                }
            }

            return default;
        }

        static public void ForEach<T>(
        this IEnumerable<T> Menge,
        Action<T, int> Aktioon)
        {
            if (null == Menge)
            {
                return;
            }

            if (null == Aktioon)
            {
                return;
            }

            int ElementIndex = 0;

            foreach (var item in Menge)
            {
                Aktioon(item, ElementIndex);

                ++ElementIndex;
            }
        }

        static public void ForEach<T>(
            this IEnumerable<T> Menge,
            Action<T> Aktioon)
        {
            if (null == Menge)
            {
                return;
            }

            if (null == Aktioon)
            {
                return;
            }

            foreach (var item in Menge)
            {
                Aktioon(item);
            }
        }

    }
}
