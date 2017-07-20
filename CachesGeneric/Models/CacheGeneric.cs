using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace CachesGeneric.Models
{
    public class CacheGeneric
    {
        internal static T GetItemToCache<T>(string key, object cacheObject, DateTime absoluteExpiration)
        {
            return GetItemToCache<T>(key, cacheObject, absoluteExpiration, Cache.NoSlidingExpiration);
        }

        internal static T GetItemToCache<T>(string key, object cacheObject, TimeSpan slidingExpiration)
        {
            return GetItemToCache<T>(key, cacheObject, Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        internal static T GetItemToCache<T>(string key, object cacheObject, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            T customObj;

            if (HttpRuntime.Cache[key] != null)
            {
                customObj = (T)HttpRuntime.Cache[key];
            }
            else
            {
                //DateTime.Now.AddMinutes(30)
                HttpRuntime.Cache.Insert(key, cacheObject, null, absoluteExpiration, slidingExpiration);
                customObj = (T)HttpRuntime.Cache[key];
            }

            return customObj;
        }
    }
}