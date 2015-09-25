using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace Google.Maps.Internal
{
    /// <summary>
    /// Represents a cached HttpGetResponse, using the internal WebInet cache for the request. 
    /// </summary>
    public class CachedHttpGetResponse : HttpGetResponse
    {
        /// <summary>
        /// Cache Policy to use when the cache shall be refreshed
        /// </summary>
        static readonly HttpRequestCachePolicy NoCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Reload);

        /// <summary>
        /// Cache Policy to be used when a cached response is permitted
        /// </summary>
        static readonly HttpRequestCachePolicy UseCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);


        /// <summary>
        /// Creates a new CachedHttpGetResponse
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="refreshCache">Do not use the cache - Fetch new data from the server and updates the cache.</param>
        public CachedHttpGetResponse(Uri uri, bool refreshCache)
            : base(uri, refreshCache)
        {
        }

        protected override string AsString()
        {
            var output = String.Empty;

            var response = GetResponse(GetSignedUri(), DontUseCache);

            if (response != null)
            {
                FromCache = response.IsFromCache;
                var rs = response.GetResponseStream();
                if (rs != null)
                {
                    using (var reader = new StreamReader(rs))
                    {
                        output = reader.ReadToEnd();
                    }
                }
            }

            return output;
        }

        protected async override Task<string> AsStringAsync()
        {
            var output = String.Empty;

            var response = await GetResponseAsync(GetSignedUri(), DontUseCache);

            if (response != null)
            {
                FromCache = response.IsFromCache;
                var rs = response.GetResponseStream();
                if (rs != null)
                {
                    using (var reader = new StreamReader(rs))
                    {
                        output = await reader.ReadToEndAsync();
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="refreshCache">Do not use cache but update it with new data</param>
        /// <returns></returns>
        private static WebResponse GetResponse(Uri uri, bool refreshCache)
        {
            var request = WebRequest.Create(uri);
            request.CachePolicy = refreshCache ? NoCachePolicy : UseCachePolicy;
            return request.GetResponse();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="refreshCache">Do not use cache but update it with new data</param>
        /// <returns></returns>
        private static Task<WebResponse> GetResponseAsync(Uri uri, bool refreshCache)
        {
            var request = WebRequest.Create(uri);
            request.CachePolicy = refreshCache ? NoCachePolicy : UseCachePolicy;
            return request.GetResponseAsync();
        }
    }
}
